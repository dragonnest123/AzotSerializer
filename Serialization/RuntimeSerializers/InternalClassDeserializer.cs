namespace AzotBase.Common.Serialization.Serializers;

internal static class InternalClassDeserializer
{
    public static T Deserialize<T>(byte[] data)
        => (T)Deserialize(typeof(T), data);

    public static object Deserialize(Type type, byte[] data)
    {
        var reader = new BinaryReader(new MemoryStream(data));
        
        return DeserializeObjectMembers(type, reader);
    }

    private static object? DeserializeObject(Type type, BinaryReader reader)
    {
        bool hasValue = reader.ReadBoolean();
        if (!hasValue)
            return null;

        var notNullableType = Nullable.GetUnderlyingType(type) ?? type;
        if (TryDeserializeBuiltInType(notNullableType, reader, out var obj))
            return obj;

        return DeserializeObjectMembers(type, reader);
    }

    private static object DeserializeObjectMembers(Type type, BinaryReader reader)
    {
        var props = TypeMetadataCache.GetProperties(type);
        var result = Activator.CreateInstance(type) 
                     ?? throw new InvalidOperationException("Could not create instance of type " + type.FullName);
        
        foreach (var accessor in props)
        {
            var deserializedValue = DeserializeObject(accessor.Type, reader);
            accessor.Setter.Invoke(result, deserializedValue);
        }

        return result;
    }

    private static bool TryDeserializeStructure(Type type, BinaryReader reader, out object? obj)
    {
        if (!type.IsValueType)
        {
            obj = null;
            return false;
        }
        
        var structBytes = reader.ReadBytes(TypeMetadataCache.GetStructSize(type));
        obj = StructSerializer.Deserialize(type, structBytes);
        return true;
    }
    
    private static bool TryDeserializeBuiltInType(Type type, BinaryReader reader, out object? obj)
    {
        if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            TryDeserializeBuiltInType(underlyingType, reader, out var enumValue);
            if (enumValue == null)
                throw new Exception("Could not deserialize Enum underlyingType");
            
            obj = Enum.ToObject(type, enumValue);
            return true;
        }
        
        obj = Type.GetTypeCode(type) switch
        {
            TypeCode.Int32   => reader.ReadInt32(),
            TypeCode.UInt32  => reader.ReadUInt32(),
            TypeCode.Int16   => reader.ReadInt16(),
            TypeCode.UInt16  => reader.ReadUInt16(),
            TypeCode.Int64   => reader.ReadInt64(),
            TypeCode.UInt64  => reader.ReadUInt64(),
            TypeCode.Single  => reader.ReadSingle(),
            TypeCode.Double  => reader.ReadDouble(),
            TypeCode.Boolean => reader.ReadBoolean(),
            TypeCode.String  => reader.ReadString(),
            TypeCode.Byte    => reader.ReadByte(),
            TypeCode.SByte   => reader.ReadSByte(),
            TypeCode.Decimal => reader.ReadDecimal(),
            TypeCode.Char    => reader.ReadChar(),
            _                => null
        };
        if (obj != null)
            return true;
        
        if (TryDeserializeStructure(type, reader, out obj))
            return true;

        return obj != null;
    }
}