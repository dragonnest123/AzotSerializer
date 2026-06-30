using System.Buffers;

namespace Serialization.RuntimeSerialization.Serializers;

public static class ObjectSerializer
{
    public static ReadOnlySpan<byte> Serialize<T>(T obj) where T : notnull
        => Serialize(typeof(T), obj);

    public static ReadOnlySpan<byte> Serialize(Type type, object obj)
        => InternalObjectSerializer.Serialize(type, obj);

    public static Action<object, ArrayBufferWriter<byte>> BuildSerializer<T>()
        => BuildSerializer(typeof(T));
    
    public static Action<object, ArrayBufferWriter<byte>> BuildSerializer(Type type)
        => InternalObjectSerializer.GetOrBuildSerializer(type);

    /// <summary>
    /// Should have parameterless constructor
    /// </summary>
    public static T Deserialize<T>(byte[] data)
    {
        var deserialized = Deserialize(typeof(T), data);
        if (deserialized == null)
            throw new InvalidOperationException();
        return (T)deserialized;
    }

    public static object? Deserialize(Type type, byte[] data)
        => InternalObjectDeserializer.Deserialize(type, data);
    
    public static DeserializerDelegate BuildDeserializer<T>()
        => BuildDeserializer(typeof(T));

    public static DeserializerDelegate BuildDeserializer(Type type)
        => InternalObjectDeserializer.GetOrBuildDeserializer(type);
}