using System.Runtime.InteropServices;

namespace Serialization.RuntimeSerializers.StructSerialization;

internal static class BlittableStructSerializer 
{
    public static Span<byte> Serialize<T>(ref T value) where T : struct
    {
        var span = MemoryMarshal.CreateSpan(ref value, 1);
        return MemoryMarshal.AsBytes(span);
    }

    public static void Serialize<T>(byte[] buffer, int offset, ref T value) where T : struct
    {
        var span = MemoryMarshal.CreateSpan(ref value, 1);
        var bytes = MemoryMarshal.AsBytes(span);
        bytes.CopyTo(buffer.AsSpan(offset));
    }
    
    public static byte[] Serialize(Type type, object value)
    {
        var size = TypeMetadata.GetStructSize(type);

        var buffer = new byte[size];
        var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
        try
        {
            Marshal.Copy(handle.AddrOfPinnedObject(), buffer, 0, size);
        }
        finally
        {
            handle.Free();
        }
        
        return buffer;
    }
    
    public static T Deserialize<T>(Span<byte> data) where T : struct
    {
        return MemoryMarshal.Read<T>(data);
    }
    
    public static object? Deserialize(Type type, byte[] data)
    {
        var result = Activator.CreateInstance(type);
        var handle = GCHandle.Alloc(result, GCHandleType.Pinned);
        try
        {
            Marshal.Copy(data, 0, handle.AddrOfPinnedObject(), data.Length);
            return result;
        }
        finally
        {
            handle.Free();
        }
    }
}