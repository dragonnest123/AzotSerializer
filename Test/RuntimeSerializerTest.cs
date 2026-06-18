using AzotBase.Common.Serialization;
using AzotBase.Common.Serialization.Serializers;
using AzotName.Tests.TestUtils.Factories;
using FluentAssertions;

namespace AzotName.Tests.Common.Serialization;

public class ClassSerializerTest
{
    [Fact]
    public void Serialize_deserialize_class_with_primitive_types_returns_same_object()
    {
        var c = RecordFactory.CreateSimpleRecord();
        
        var serialized = ClassSerializer.Serialize(c);
        var deserialized = ClassSerializer.Deserialize<SimpleRecord>(serialized.ToArray());
        
        deserialized.Should().BeEquivalentTo(c);
    }
    
    [Fact]
    public void Serialize_deserialize_class_with_nullable_primitive_types_returns_same_object()
    {
        var c = RecordFactory.CreateSimpleRecord(name: null);
        
        var serialized = ClassSerializer.Serialize(c);
        var deserialized = ClassSerializer.Deserialize<SimpleRecord>(serialized.ToArray());
        
        deserialized.Should().BeEquivalentTo(c);
    }
    
    [Fact]
    public void Serialize_deserialize_class_with_nested_class_and_struct_returns_same_object()
    {
        var c = RecordFactory.CreateComplexRecord();
        
        var serialized = ClassSerializer.Serialize(c);
        var deserialized = ClassSerializer.Deserialize<ComplexRecord>(serialized.ToArray());
        
        deserialized.Should().BeEquivalentTo(c);
    }
}