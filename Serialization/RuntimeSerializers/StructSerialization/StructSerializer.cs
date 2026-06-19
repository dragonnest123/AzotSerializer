using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Serialization.RuntimeSerializers.StructSerialization;

public static class StructSerializer
{
    public static Span<byte> Serialize<T>(ref T value)  where T : struct
    {
        if (TypeMetadata.IsBlittable(typeof(T)))
            return BlittableStructSerializer.Serialize(ref value);
    }

    public static void Serialize<T>(byte[] buffer, int offset, ref T value) where T : struct
    {
        if (TypeMetadata.IsBlittable(typeof(T)))
        {
            BlittableStructSerializer.Serialize(buffer, offset, ref value);
            return;
        }
        
        
    }
    
    public static byte[] Serialize(Type type, object value)
    {
        if (TypeMetadata.IsBlittable(type))
            return BlittableStructSerializer.Serialize(type, value);
    }
    
    public static T Deserialize<T>(Span<byte> data) where T : struct
    {
        
    }
    
    public static object? Deserialize(Type type, byte[] data)
    {

    }
}