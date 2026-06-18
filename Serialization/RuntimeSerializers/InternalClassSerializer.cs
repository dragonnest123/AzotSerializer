using System.Buffers;
using Serialization.Extensions;

namespace AzotBase.Common.Serialization.Serializers;

internal static class InternalClassSerializer
{
    public static ReadOnlySpan<byte> Serialize<T>(T obj) where T : notnull
        => Serialize(typeof(T), obj);

    public static ReadOnlySpan<byte> Serialize(Type type, object obj)
    {
        var writer = new ArrayBufferWriter<byte>();

        SerializeObjectMembers(type, obj, writer);

        return writer.WrittenSpan;
    }
    
    private static void SerializeObject(Type type, object? obj, ArrayBufferWriter<byte> writer)
    {
        if (obj is null)
        {
            writer.WriteByte(0);
            return;
        }
        writer.WriteByte(1);

        var notNullableType = Nullable.GetUnderlyingType(type) ?? type;
        if (TrySerializeBuiltInType(notNullableType, obj, writer))
            return;

        SerializeObjectMembers(type, obj, writer);
    }

    private static void SerializeObjectMembers(Type type, object obj, ArrayBufferWriter<byte> writer)
    {
        var props = TypeMetadataCache.GetProperties(type);
        foreach (var accessor in props)
        {
            var prop = accessor.Getter.Invoke(obj);
            SerializeObject(accessor.Type, prop, writer);
        }
    }
    
    private static bool TrySerializeStructure(Type type, object obj, ArrayBufferWriter<byte> writer)
    {
        if (!type.IsValueType)
            return false;
        
        var serialized = StructSerializer.Serialize(type, obj);
        writer.Write(serialized);
        return true;
    }
    
    private static bool TrySerializeBuiltInType(Type type, object obj, ArrayBufferWriter<byte> writer)
    {
        if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            obj = Convert.ChangeType(obj, underlyingType);
        }
        
        switch (obj)
        {
            case bool b:    writer.WriteBool(b);    return true;
            case byte b:    writer.WriteByte(b);    return true;
            case sbyte b:   writer.WriteSByte(b);   return true;
            case short s:   writer.WriteInt16(s);   return true;
            case ushort s:  writer.WriteUInt16(s);  return true;
            case char c:    writer.WriteUInt16(c);  return true;
            case int i:     writer.WriteInt32(i);   return true;
            case uint i:    writer.WriteUInt32(i);  return true;
            case long l:    writer.WriteInt64(l);   return true;
            case ulong l:   writer.WriteUInt64(l);  return true;
            case float f:   writer.WriteSingle(f);  return true;
            case double d:  writer.WriteDouble(d);  return true;
            case decimal d: writer.WriteDecimal(d); return true;
            case string s:  writer.WriteString(s);  return true;
        }
        
        if (TrySerializeStructure(type, obj, writer))
            return true;
        
        return false;
    }
}