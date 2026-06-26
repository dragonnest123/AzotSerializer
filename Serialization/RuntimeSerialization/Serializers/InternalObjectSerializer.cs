using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Serialization.Extensions;

namespace Serialization.RuntimeSerialization.Serializers;

internal static class InternalObjectSerializer
{
    private static readonly Dictionary<Type, Action<object, ArrayBufferWriter<byte>>> _cacheSerializers = new();
    private static readonly ThreadLocal<HashSet<object>> _visited = 
        new(() => new HashSet<object>(ReferenceEqualityComparer.Instance));

    public static ReadOnlySpan<byte> Serialize(Type type, object obj)
    {
        var writer = new ArrayBufferWriter<byte>();

        GetOrBuildSerializer(type)(obj, writer);
        
        return writer.WrittenSpan;
    }
    
    public static Action<object, ArrayBufferWriter<byte>> GetOrBuildSerializer(Type type)
    {
        if (_cacheSerializers.TryGetValue(type, out var cachedSerializer))
            return cachedSerializer;
        
        Action<object, ArrayBufferWriter<byte>>? serializer = null;
        _cacheSerializers[type] = (obj, writer) => serializer(obj, writer);

        serializer = BuildObjectSerializer(type);

        return serializer;
    }
    
    private static Action<object, ArrayBufferWriter<byte>> BuildObjectSerializer(Type type)
    {
        var notNullableType = Nullable.GetUnderlyingType(type) ?? type;
    
        if (TryBuildSupportedTypeSerializer(notNullableType, out var serializer) 
            || SpecificTypeSerializer.TryBuildSpecificTypeSerializer(notNullableType, out serializer) 
            || TryBuildCollectionSerializer(notNullableType, out serializer))
            return serializer;

        return BuildMembersSerializer(type);
    }
    
    private static Action<object, ArrayBufferWriter<byte>> BuildMembersSerializer(Type type)
    {
        var memberSerializers = type
            .GetMembers()
            .Where(x => x is PropertyInfo or FieldInfo)
            .Select(BuildMemberSerializer)
            .ToArray();
        
        return (obj, writer) =>
        {
            if (!_visited.Value!.Add(obj))
                return;

            try
            {
                foreach (var serializer in memberSerializers)
                    serializer(obj, writer);
            }
            finally
            {
                _visited.Value!.Remove(obj);
            }
        };
    }
    
    private static Action<object?, ArrayBufferWriter<byte>> BuildMemberSerializer(MemberInfo member)
    {
        var objParam = Expression.Parameter(typeof(object), "obj");
        var writerParam = Expression.Parameter(typeof(ArrayBufferWriter<byte>), "writer");
        
        var castedObject = member.DeclaringType!.IsValueType 
            ? Expression.Unbox(objParam, member.DeclaringType) 
            : Expression.Convert(objParam, member.DeclaringType);
        
        var memberExp = Expression.PropertyOrField(castedObject, member.Name);
        var notNullableType = Nullable.GetUnderlyingType(memberExp.Type) ?? memberExp.Type;
        
        var writeByteMethod = typeof(BufferWriterExtensions).GetMethod(nameof(BufferWriterExtensions.WriteByte))
            ?? throw new ArgumentException($"{nameof(BufferWriterExtensions)} doesn't contain WriteByte method");
        
        var serializer = Expression.Constant(GetOrBuildSerializer(notNullableType));
        var serializerInvoke = Expression.Invoke(
            serializer, 
            Expression.Convert(memberExp, typeof(object)), 
            writerParam);

        Expression body;
        
        bool canBeNull = !memberExp.Type.IsValueType || Nullable.GetUnderlyingType(memberExp.Type) is not null;
        if (canBeNull)
            body = Expression.IfThenElse(
                Expression.Equal(memberExp, Expression.Constant(null)),
                Expression.Call(writeByteMethod, writerParam, Expression.Constant((byte)0)),
                Expression.Block(
                    Expression.Call(writeByteMethod, writerParam, Expression.Constant((byte)1)),
                    serializerInvoke));
        else
            body = serializerInvoke;
                
        return Expression.Lambda<Action<object?, ArrayBufferWriter<byte>>>(body, objParam, writerParam)
            .Compile();
    }
    
    private static bool TryBuildSupportedTypeSerializer(
        Type type, 
        [NotNullWhen(true)] out Action<object, ArrayBufferWriter<byte>>? serializer)
    {
        if (type == typeof(bool))        { serializer = (o, w) => w.WriteBool((bool)o);         return true; }
        if (type == typeof(byte))        { serializer = (o, w) => w.WriteByte((byte)o);         return true; }
        if (type == typeof(sbyte))       { serializer = (o, w) => w.WriteSByte((sbyte)o);       return true; }
        if (type == typeof(short))       { serializer = (o, w) => w.WriteInt16((short)o);       return true; }
        if (type == typeof(ushort))      { serializer = (o, w) => w.WriteUInt16((ushort)o);     return true; }
        if (type == typeof(char))        { serializer = (o, w) => w.WriteChar((char)o);         return true; }
        if (type == typeof(int))         { serializer = (o, w) => w.WriteInt32((int)o);         return true; }
        if (type == typeof(uint))        { serializer = (o, w) => w.WriteUInt32((uint)o);       return true; }
        if (type == typeof(long))        { serializer = (o, w) => w.WriteInt64((long)o);        return true; }
        if (type == typeof(ulong))       { serializer = (o, w) => w.WriteUInt64((ulong)o);      return true; }
        if (type == typeof(float))       { serializer = (o, w) => w.WriteSingle((float)o);      return true; }
        if (type == typeof(double))      { serializer = (o, w) => w.WriteDouble((double)o);     return true; }
        if (type == typeof(decimal))     { serializer = (o, w) => w.WriteDecimal((decimal)o);   return true; }
        if (type == typeof(string))      { serializer = (o, w) => w.WriteString((string)o);     return true; }
        if (type == typeof(DateTime))    { serializer = (o, w) => w.WriteDateTime((DateTime)o); return true; }
        if (type == typeof(TimeSpan))    { serializer = (o, w) => w.WriteTimeSpan((TimeSpan)o); return true; }

        if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            var underlyingSerializer = GetOrBuildSerializer(underlyingType);
            serializer = (o, w) => underlyingSerializer(Convert.ChangeType(o, underlyingType), w);
            return true;
        }

        if (type.IsArray)
        {
            serializer = BuildArraySerializer(type);
            return true;
        }

        serializer = null;
        return false;
    }

    private static Action<object, ArrayBufferWriter<byte>> BuildArraySerializer(Type arrayType)
    {
        var elementType = arrayType.GetElementType()
            ?? throw new Exception("Unknown array type");
        
        var elementSerializer = GetOrBuildSerializer(elementType);
        var enumerableSerializer = BuildEnumerableSerializer(elementType, elementSerializer);
        return (obj, writer) =>
        {
            writer.WriteInt32(((Array)obj).Length);
            enumerableSerializer(obj, writer);
        };
    }

    private static bool TryBuildCollectionSerializer(
        Type type, 
        [NotNullWhen(true)] out Action<object, ArrayBufferWriter<byte>>? serializer)
    {
        if (!TypeDataProvider.IsCollection(type, out var collectionType))
        {
            serializer = null;   
            return false;
        }
        
        var elementType = collectionType.GetGenericArguments()[0];
        var elementSerializer = GetOrBuildSerializer(elementType);

        var countGetter = BuildCountPropertyDelegate(elementType);
        var enumerableSerializer = BuildEnumerableSerializer(elementType, elementSerializer);
        serializer = (obj, writer) =>
        {
            writer.WriteInt32(countGetter(obj));
            enumerableSerializer(obj, writer);
        };

        return true;
    }
    
    private static Action<object, ArrayBufferWriter<byte>> BuildEnumerableSerializer(
        Type elementType,
        Action<object, ArrayBufferWriter<byte>> elementSerializer)
    {
        bool canBeNull = TypeDataProvider.CanBeNull(elementType);
    
        if (!canBeNull)
            return (obj, writer) =>
            {
                foreach (var element in (IEnumerable)obj)
                    elementSerializer(element, writer);
            };

        return (obj, writer) =>
        {
            foreach (var element in (IEnumerable)obj)
            {
                if (element is null) 
                { 
                    writer.WriteByte(0); 
                    continue; 
                }
                writer.WriteByte(1);
                
                elementSerializer(element, writer);
            }
        };
    }

    private static Func<object, int> BuildCountPropertyDelegate(Type elementType)
    {
        var collectionType = typeof(ICollection<>).MakeGenericType(elementType);

        return MemberAccessor.BuildGetDelegate<int>(collectionType, "Count");
    }
}