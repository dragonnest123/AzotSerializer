using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Serialization.RuntimeSerialization.Serializers;

internal static class UnmanagedStructSerializer 
{
    public static ReadOnlySpan<byte> Serialize<T>(ref T value) where T : struct
    {
        var bytes = new byte[Unsafe.SizeOf<T>()];
        MemoryMarshal.Write(bytes, value);
        return bytes;
    }

    public static void Serialize<T>(byte[] buffer, int offset, ref T value) where T : struct
    {
        MemoryMarshal.Write(buffer.AsSpan(offset), value);
    }
    
    public static ReadOnlySpan<byte> Serialize(Type type, object value)
    {
        var size = TypeDataProvider.GetStructSize(type);

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
    
    public static T Deserialize<T>(byte[] data) where T : struct
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