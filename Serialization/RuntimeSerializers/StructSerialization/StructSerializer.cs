using Serialization.RuntimeSerializers.ObjectSerialization;

namespace Serialization.RuntimeSerializers.StructSerialization;

public static class StructSerializer
{
    public static ReadOnlySpan<byte> Serialize<T>(ref T value) where T : struct
    {
        if (TypeMetadata.IsUnmanaged<T>())
            return UnmanagedStructSerializer.Serialize(typeof(T), value);

        return ObjectSerializer.Serialize(value);
    }
    
    public static ReadOnlySpan<byte> Serialize(Type type, object value)
    {
        if (TypeMetadata.IsUnmanaged(type))
            return UnmanagedStructSerializer.Serialize(type, value);

        return ObjectSerializer.Serialize(type, value);
    }
    
    public static void Serialize<T>(byte[] buffer, int offset, ref T value) where T : struct
    {
        if (TypeMetadata.IsUnmanaged<T>())
        {
            UnmanagedStructSerializer.Serialize(buffer, offset, ref value);
            return;
        }

        throw new NotImplementedException();
    }

    public static T Deserialize<T>(byte[] data) where T : struct
    {
        if (TypeMetadata.IsUnmanaged<T>())
            return UnmanagedStructSerializer.Deserialize<T>(data);
        
        return ObjectSerializer.Deserialize<T>(data);
    }
        
    
    public static object? Deserialize(Type type, byte[] data)
    {
        if (TypeMetadata.IsUnmanaged(type))
            return UnmanagedStructSerializer.Deserialize(type, data);
        
        return ObjectSerializer.Deserialize(type, data);
    }
}