using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Serialization.Extensions;

namespace Serialization.RuntimeSerialization.Serializers;

internal delegate object? DeserializerDelegate(ref ReadOnlySpan<byte> buffer);

internal static class InternalObjectDeserializer
{
    private delegate void MemberDeserializerDelegate(object target, ref ReadOnlySpan<byte> buffer);
    
    private static readonly Dictionary<Type, DeserializerDelegate> _cacheDeserializers = new();

    public static object? Deserialize(Type type, byte[] data)
    {
        ReadOnlySpan<byte> span = data.AsSpan();
        return GetOrBuildDeserializer(type)(ref span);
    }

    public static DeserializerDelegate GetOrBuildDeserializer(Type type)
    {
        if (_cacheDeserializers.TryGetValue(type, out var cached))
            return cached;

        DeserializerDelegate? deserializer = null;
        _cacheDeserializers[type] = (ref buffer) => deserializer!(ref buffer);

        deserializer = BuildObjectDeserializer(type);

        return deserializer;
    }

    private static DeserializerDelegate BuildObjectDeserializer(Type type)
    {
        var notNullableType = Nullable.GetUnderlyingType(type) ?? type;

        if (TryBuildSupportedTypeDeserializer(notNullableType, out var deserializer)
            || SpecificTypeSerializer.TryBuildSpecificTypeDeserializer(notNullableType, out deserializer)
            || TryBuildCollectionDeserializer(notNullableType, out deserializer))
            return deserializer;

        return BuildMembersDeserializer(notNullableType);
    }

    private static DeserializerDelegate BuildMembersDeserializer(Type type)
    {
        var members = type
            .GetMembers()
            .Where(x => x is PropertyInfo or FieldInfo)
            .Select(BuildMemberDeserializer)
            .ToArray();

        return (ref buffer) =>
        {
            var result = Activator.CreateInstance(type)
                         ?? throw new InvalidOperationException("Could not create instance of type " + type.FullName);

            foreach (var memberDeserializer in members)
                memberDeserializer(result, ref buffer);

            return result;
        };
    }

    private static MemberDeserializerDelegate BuildMemberDeserializer(MemberInfo member)
    {
        var targetParam = Expression.Parameter(typeof(object), "target");
        var bufferParam = Expression.Parameter(typeof(ReadOnlySpan<byte>).MakeByRefType(), "buffer");

        var memberType = member switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => throw new ArgumentException("Unsupported member type")
        };

        var notNullableType = Nullable.GetUnderlyingType(memberType) ?? memberType;
        bool canBeNull = !memberType.IsValueType || Nullable.GetUnderlyingType(memberType) != null;

        var innerDeserializer = Expression.Constant(GetOrBuildDeserializer(notNullableType));

        var castedTarget = member.DeclaringType!.IsValueType
            ? Expression.Unbox(targetParam, member.DeclaringType)
            : Expression.Convert(targetParam, member.DeclaringType);

        var memberExp = Expression.PropertyOrField(castedTarget, member.Name);

        var deserializerInvoke = Expression.Convert(
            Expression.Invoke(innerDeserializer, bufferParam),
            memberType);

        Expression body;

        if (canBeNull)
        {
            var readBoolMethod = typeof(SpanReaderExtensions).GetMethod(nameof(SpanReaderExtensions.ReadBool))
                                 ?? throw new ArgumentException("ReadBool method not found");

            var hasValue = Expression.Call(readBoolMethod, bufferParam);

            body = Expression.IfThenElse(
                hasValue,
                Expression.Assign(memberExp, deserializerInvoke),
                Expression.Assign(memberExp, Expression.Default(memberType)));
        }
        else
            body = Expression.Assign(memberExp, deserializerInvoke);
        
        return Expression.Lambda<MemberDeserializerDelegate>(body, targetParam, bufferParam)
            .Compile();
    }


    private static bool TryBuildSupportedTypeDeserializer(
        Type type,
        [NotNullWhen(true)] out DeserializerDelegate? deserializer)
    {
        if (type == typeof(bool))     { deserializer = (ref b) => b.ReadBool();     return true; }
        if (type == typeof(byte))     { deserializer = (ref b) => b.ReadByte();     return true; }
        if (type == typeof(sbyte))    { deserializer = (ref b) => b.ReadSByte();    return true; }
        if (type == typeof(short))    { deserializer = (ref b) => b.ReadInt16();    return true; }
        if (type == typeof(ushort))   { deserializer = (ref b) => b.ReadUInt16();   return true; }
        if (type == typeof(char))     { deserializer = (ref b) => b.ReadChar();     return true; }
        if (type == typeof(int))      { deserializer = (ref b) => b.ReadInt32();    return true; }
        if (type == typeof(uint))     { deserializer = (ref b) => b.ReadUInt32();   return true; }
        if (type == typeof(long))     { deserializer = (ref b) => b.ReadInt64();    return true; }
        if (type == typeof(ulong))    { deserializer = (ref b) => b.ReadUInt64();   return true; }
        if (type == typeof(float))    { deserializer = (ref b) => b.ReadSingle();   return true; }
        if (type == typeof(double))   { deserializer = (ref b) => b.ReadDouble();   return true; }
        if (type == typeof(decimal))  { deserializer = (ref b) => b.ReadDecimal();  return true; }
        if (type == typeof(string))   { deserializer = (ref b) => b.ReadString();   return true; }
        if (type == typeof(DateTime)) { deserializer = (ref b) => b.ReadDateTime(); return true; }
        if (type == typeof(TimeSpan)) { deserializer = (ref b) => b.ReadTimeSpan(); return true; }

        if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            var underlyingDeserializer = GetOrBuildDeserializer(underlyingType);
            deserializer = (ref b) =>
            {
                var value = underlyingDeserializer(ref b)
                            ?? throw new Exception("Could not deserialize Enum");
                return Enum.ToObject(type, value);
            };
            return true;
        }

        if (type.IsArray)
        {
            deserializer = BuildArrayDeserializer(type);
            return true;
        }

        deserializer = null;
        return false;
    }

    private static bool TryBuildCollectionDeserializer(
        Type type,
        [NotNullWhen(true)] out DeserializerDelegate? deserializer)
    {
        if (!TypeDataProvider.IsCollection(type, out var collectionType))
        {
            deserializer = null;
            return false;
        }

        var elementType = collectionType.GetGenericArguments()[0];
        var elementDeserializer = GetOrBuildDeserializer(elementType);
        bool elementCanBeNull = TypeDataProvider.CanBeNull(elementType);
        
        var addItem = BuildCollectionAddDelegate(elementType);

        deserializer = (ref buffer) =>
        {
            int count = buffer.ReadInt32();
            var collection = Activator.CreateInstance(type)!;

            if (elementCanBeNull)
            {
                for (int i = 0; i < count; i++)
                {
                    bool hasValue = buffer.ReadBool();
                    addItem(collection, hasValue ? elementDeserializer(ref buffer) : null);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                    addItem(collection, elementDeserializer(ref buffer));
            }

            return collection;
        };
        
        return true;
    }
    
    private static DeserializerDelegate BuildArrayDeserializer(Type arrayType)
    {
        var elementType = arrayType.GetElementType()
                          ?? throw new Exception("Unknown array type");
        
        var elementDeserializer = GetOrBuildDeserializer(elementType);
        bool elementCanBeNull = TypeDataProvider.CanBeNull(elementType);

        return (ref buffer) =>
        {
            int length = buffer.ReadInt32();
            var array = Array.CreateInstance(elementType, length);
            
            if (elementCanBeNull)
            {
                for (int i = 0; i < length; i++)
                {
                    bool hasValue = buffer.ReadBool();
                    array.SetValue(hasValue ? elementDeserializer(ref buffer) : null, i);
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                    array.SetValue(elementDeserializer(ref buffer), i);
            }

            return array;
        };
    }
    
    private static Action<object, object?> BuildCollectionAddDelegate(Type elementType)
    {
        var collectionParam = Expression.Parameter(typeof(object));
        var itemParam = Expression.Parameter(typeof(object));

        var castCollection = Expression.Convert(
            collectionParam,
            typeof(ICollection<>).MakeGenericType(elementType));

        var castItem = Expression.Convert(itemParam, elementType);

        var addMethod = typeof(ICollection<>)
            .MakeGenericType(elementType)
            .GetMethod(nameof(ICollection<>.Add))!;

        var body = Expression.Call(
            castCollection,
            addMethod,
            castItem);

        return Expression
            .Lambda<Action<object, object?>>(
                body,
                collectionParam,
                itemParam)
            .Compile();
    }
}