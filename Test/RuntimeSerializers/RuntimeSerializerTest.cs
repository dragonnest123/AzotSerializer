using FluentAssertions;
using Serialization.RuntimeSerializers;

namespace Test;

public class RuntimeSerializerTest
{
    [Fact]
    public void Serialize_SimpleClass()
    {
        var c = ClassFactory.CreateSimpleClass();
        
        var serialized = ObjectSerializer.Serialize(c);
        var deserialized = ObjectSerializer.Deserialize<SimpleClass>(serialized.ToArray());
        
        deserialized.Should().BeEquivalentTo(c);
    }
    
    [Fact]
    public void Serialize_ClassWithNestedObjects()
    {
        var c = ClassFactory.CreateClassWithNestedObjects();
        
        var serialized = ObjectSerializer.Serialize(c);
        var deserialized = ObjectSerializer.Deserialize<ClassWithNestedObjects>(serialized.ToArray());
        
        deserialized.Should().BeEquivalentTo(c);
    }
    
    [Fact]
    public void Serialize_ClassWithCollection()
    {
        var c = ClassFactory.CreateClassWithCollection();
        
        var serialized = ObjectSerializer.Serialize(c);
        var deserialized = ObjectSerializer.Deserialize<ClassWithCollection>(serialized.ToArray());
        
        deserialized.Should().BeEquivalentTo(c);
    }
}