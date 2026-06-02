using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using Serialization.Extensions;

namespace Experimental;

public partial class User
{
    public User() { }

    public static void Serialize(User value, IBufferWriter<byte> writer)
    {
                writer.WriteInt32(value.Id);
        writer.WriteString(value.Name);
        writer.WriteString(value.Email);

    }
    
    public static ReadOnlySpan<byte> Serialize(User value)
    {
        var writer = new ArrayBufferWriter<byte>();
        Serialize(value, writer);
        
        return writer.WrittenSpan;
    }
    

    public static User Deserialize(ref ReadOnlySpan<byte> buffer)
    {
                var deserializedClass = new User();
        deserializedClass.Id = buffer.ReadInt32();;
        deserializedClass.Name = buffer.ReadString();;
        deserializedClass.Email = buffer.ReadString();;
        return deserializedClass;

    }
}