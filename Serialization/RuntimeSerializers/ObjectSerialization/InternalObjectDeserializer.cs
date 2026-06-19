using Serialization.Extensions;
using Serialization.RuntimeSerializers.StructSerialization;

namespace Serialization.RuntimeSerializers.ObjectSerialization;

internal static class InternalObjectDeserializer
{
    public static T Deserialize<T>(byte[] data)
        => (T)Deserialize(typeof(T), data);

    public static object Deserialize(Type type, byte[] data)
    {
        ReadOnlySpan<byte> span = data.AsSpan();
        
        return DeserializeObjectMembers(type, ref span);
    }  

    private static object? DeserializeObject(Type type, ref ReadOnlySpan<byte> buffer)
    {
        bool hasValue = buffer.ReadBool();
        if (!hasValue)
            return null;

        var notNullableType = Nullable.GetUnderlyingType(type) ?? type;
        if (TryDeserializeBuiltInType(notNullableType, ref buffer, out var obj))
            return obj;

        return DeserializeObjectMembers(notNullableType, ref buffer);
    }

    private static object DeserializeObjectMembers(Type type, ref ReadOnlySpan<byte> buffer)
    {
        var props = TypeMetadata.GetProperties(type);
        var result = Activator.CreateInstance(type) 
                     ?? throw new InvalidOperationException("Could not create instance of type " + type.FullName);
        
        foreach (var accessor in props)
        {
            var deserializedValue = DeserializeObject(accessor.Type, ref buffer);
            accessor.Setter.Invoke(result, deserializedValue);
        }

        return result;
    }

    private static bool TryDeserializeStructure(Type type, ref ReadOnlySpan<byte> buffer, out object? obj)
    {
        if (!type.IsValueType)
        {
            obj = null;
            return false;
        }
        
        var structBytes = buffer.ReadBytes(TypeMetadata.GetStructSize(type));
        obj = StructSerializer.Deserialize(type, structBytes.ToArray());
        return true;
    }
    
    private static bool TryDeserializeBuiltInType(Type type, ref ReadOnlySpan<byte> buffer, out object? obj)
    {
        if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            TryDeserializeBuiltInType(underlyingType, ref buffer, out var enumValue);
            if (enumValue == null)
                throw new Exception("Could not deserialize Enum underlyingType");
            
            obj = Enum.ToObject(type, enumValue);
            return true;
        }
        
        obj = Type.GetTypeCode(type) switch
        {
            TypeCode.Int32   => buffer.ReadInt32(),
            TypeCode.UInt32  => buffer.ReadUInt32(),
            TypeCode.Int16   => buffer.ReadInt16(),
            TypeCode.UInt16  => buffer.ReadUInt16(),
            TypeCode.Int64   => buffer.ReadInt64(),
            TypeCode.UInt64  => buffer.ReadUInt64(),
            TypeCode.Single  => buffer.ReadSingle(),
            TypeCode.Double  => buffer.ReadDouble(),
            TypeCode.Boolean => buffer.ReadBool(),
            TypeCode.String  => buffer.ReadString(),
            TypeCode.Byte    => buffer.ReadByte(),
            TypeCode.SByte   => buffer.ReadSByte(),
            TypeCode.Decimal => buffer.ReadDecimal(),
            TypeCode.Char    => buffer.ReadChar(),
            _                => null
        };
        if (obj != null)
            return true;
        
        if (TryDeserializeStructure(type, ref buffer, out obj))
            return true;

        return obj != null;
    }
}