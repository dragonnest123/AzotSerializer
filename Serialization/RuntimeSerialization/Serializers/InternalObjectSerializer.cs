using System.Buffers;
using Serialization.Extensions;
using Serialization.RuntimeSerializers.StructSerialization;

namespace Serialization.RuntimeSerializers.ObjectSerialization;

internal static class InternalObjectSerializer
{
    public static ReadOnlySpan<byte> Serialize(Type type, object obj)
    {
        var writer = new ArrayBufferWriter<byte>();
        
        SerializeObject(type, obj, writer);

        return writer.WrittenSpan;
    }
    
    private static void SerializeObject(Type type, object obj, ArrayBufferWriter<byte> writer)
    {
        var notNullableType = Nullable.GetUnderlyingType(type) ?? type;
    
        if (TrySerializeSupportedType(notNullableType, obj, writer))
            return;

        SerializeMembers(notNullableType, obj, writer);
    }
    
    private static void SerializeMembers(Type type, object obj, ArrayBufferWriter<byte> writer)
    {
        var props = TypeMetadata.GetMembers(type);
        foreach (var accessor in props)
        {
            var prop = accessor.Getter.Invoke(obj);
            SerializeMember(accessor.Type, prop, writer);
        }
    }
    
    private static void SerializeMember(Type type, object? obj, ArrayBufferWriter<byte> writer)
    {
        if (obj is null)
        {
            writer.WriteByte(0);
            return;
        }
        writer.WriteByte(1);

        SerializeObject(type, obj, writer); 
    }
    
    private static bool TrySerializeStructure(Type type, object obj, ArrayBufferWriter<byte> writer)
    {
        if (!type.IsValueType)
            return false;
        
        var serialized = StructSerializer.Serialize(type, obj);
        writer.Write(serialized);
        return true;
    }
    
    private static bool TrySerializeSupportedType(Type type, object obj, ArrayBufferWriter<byte> writer)
    {
        if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            obj = Convert.ChangeType(obj, underlyingType);
        }
        
        switch (obj)
        {
            case bool b:       writer.WriteBool(b);       return true;
            case byte b:       writer.WriteByte(b);       return true;
            case sbyte b:      writer.WriteSByte(b);      return true;
            case short s:      writer.WriteInt16(s);      return true;
            case ushort s:     writer.WriteUInt16(s);     return true;
            case char c:       writer.WriteChar(c);       return true;
            case int i:        writer.WriteInt32(i);      return true;
            case uint i:       writer.WriteUInt32(i);     return true;
            case long l:       writer.WriteInt64(l);      return true;
            case ulong l:      writer.WriteUInt64(l);     return true;
            case float f:      writer.WriteSingle(f);     return true;
            case double d:     writer.WriteDouble(d);     return true;
            case decimal d:    writer.WriteDecimal(d);    return true;
            case string s:     writer.WriteString(s);     return true;
            case DateTime dt:  writer.WriteDateTime(dt);  return true;
            case TimeSpan ts:  writer.WriteTimeSpan(ts);  return true;
        }
        
        // if (TrySerializeStructure(type, obj, writer))
        //     return true;
        
        return false;
    }
}