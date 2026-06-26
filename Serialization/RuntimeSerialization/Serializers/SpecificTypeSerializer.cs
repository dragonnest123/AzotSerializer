using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Serialization.Extensions;

namespace Serialization.RuntimeSerialization.Serializers;

internal static class SpecificTypeSerializer
{
    private static readonly Dictionary<
        Type, 
        Func<Type, (Action<object, ArrayBufferWriter<byte>> Serializer, DeserializerDelegate Deserializer)>> 
    _handlerFactories = new()
    {
        [typeof(KeyValuePair<,>)] = kvpType => (
            BuildKeyValuePairSerializer(kvpType), 
            BuildKeyValuePairDeserializer(kvpType)),
    };
    
    public static bool TryBuildSpecificTypeSerializer(
        Type type,
        [NotNullWhen(true)] out Action<object, ArrayBufferWriter<byte>>? serializer)
    {
        var genericTypeDefinition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        
        if (!_handlerFactories.TryGetValue(genericTypeDefinition, out var factory))
        {
            serializer = null;
            return false;
        }
        
        serializer = factory(type).Serializer;
        return true;
    }
    
    public static bool TryBuildSpecificTypeDeserializer(
        Type type,
        [NotNullWhen(true)] out DeserializerDelegate? deserializer)
    {
        var genericTypeDefinition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        
        if (!_handlerFactories.TryGetValue(genericTypeDefinition, out var factory))
        {
            deserializer = null;
            return false;
        }
        
        deserializer = factory(type).Deserializer;
        return true;
    }
    
    private static Action<object, ArrayBufferWriter<byte>> BuildKeyValuePairSerializer(Type kvpType)
    {
        var args = kvpType.GetGenericArguments();
        var keyType = args[0];
        var valueType = args[1];
        
        var keySerializer = SerializationHelper.WrapSerializerWithNullCheck(
            InternalObjectSerializer.GetOrBuildSerializer(keyType), keyType);
        var valueSerializer =  SerializationHelper.WrapSerializerWithNullCheck(
            InternalObjectSerializer.GetOrBuildSerializer(valueType), valueType);

        var getKey = MemberAccessor.BuildGetDelegate<object>(kvpType, "Key");
        var getValue = MemberAccessor.BuildGetDelegate<object>(kvpType, "Value");
        
        return (kvp, writer) =>
        {
            keySerializer(getKey(kvp), writer);
            valueSerializer(getValue(kvp), writer);
        };
    }
    
    private static DeserializerDelegate BuildKeyValuePairDeserializer(Type kvpType)
    {
        var args = kvpType.GetGenericArguments();
        var keyType = args[0];
        var valueType = args[1];

        var keyDeserializer = SerializationHelper.WrapDeserializerWithNullCheck(
            InternalObjectDeserializer.GetOrBuildDeserializer(keyType), keyType);
        var valueDeserializer = SerializationHelper.WrapDeserializerWithNullCheck(
            InternalObjectDeserializer.GetOrBuildDeserializer(valueType), valueType);
        
        return (ref buffer) =>
        {
            var key = keyDeserializer(ref buffer);
            var value = valueDeserializer(ref buffer);

            return Activator.CreateInstance(kvpType, key, value);
        };
    }
}