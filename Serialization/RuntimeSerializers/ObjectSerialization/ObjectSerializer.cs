namespace Serialization.RuntimeSerializers.ObjectSerialization;

public static class ObjectSerializer
{
    public static ReadOnlySpan<byte> Serialize<T>(T obj) where T : notnull
        => Serialize(typeof(T), obj);

    public static ReadOnlySpan<byte> Serialize(Type type, object obj)
        => InternalObjectSerializer.Serialize(type, obj);

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
}