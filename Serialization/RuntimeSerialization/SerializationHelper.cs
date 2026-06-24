using System.Buffers;
using Serialization.Extensions;
using Serialization.RuntimeSerialization.Serializers;

namespace Serialization.RuntimeSerialization;

internal static class SerializationHelper
{
    public static Action<object, ArrayBufferWriter<byte>> WrapSerializerWithNullCheck(
        Action<object, ArrayBufferWriter<byte>> innerSerializer, Type memberType)
    {
        if (!TypeDataProvider.CanBeNull(memberType))
            return innerSerializer;

        return (obj, writer) =>
        {
            if (obj is null)
            {
                writer.WriteByte(0); 
                return;
            }
            writer.WriteByte(1);
            
            innerSerializer(obj, writer);
        };
    }
    
    public static DeserializerDelegate WrapDeserializerWithNullCheck(
        DeserializerDelegate innerDeserializer, Type memberType)
    {
        if (!TypeDataProvider.CanBeNull(memberType))
            return innerDeserializer;

        return (ref buffer) =>
        {
            bool hasValue = buffer.ReadBool();
            if (!hasValue)
                return null;

            return innerDeserializer(ref buffer);
        };
    }
}