namespace AzotBase.Common.Serialization.Serializers;

public static class ClassSerializer
{
    public static ReadOnlySpan<byte> Serialize<T>(T obj) where T : notnull
        => Serialize(typeof(T), obj);

    public static ReadOnlySpan<byte> Serialize(Type type, object obj)
        => InternalClassSerializer.Serialize(type, obj);
    
    /// <summary>
    /// Should have parameterless constructor
    /// </summary>
    public static T Deserialize<T>(byte[] data)
        => (T)Deserialize(typeof(T), data);

    public static object Deserialize(Type type, byte[] data)
        => InternalClassDeserializer.Deserialize(type, data);
}