using FluentAssertions;

namespace Test;

public class SerializerTest
{
    [Fact]
    public void Serialize_SimpleClass()
    {
        var simpleClass = ClassFactory.CreateSimpleClass();
        
        var serialized = simpleClass.Serialize();
        var deserialized = SimpleClass.Deserialize(ref serialized);
        
        simpleClass.Should().BeEquivalentTo(deserialized);
    }

    [Fact]
    public void Serialize_ClassWithNestedClass()
    {
        var classWithNestedClass = ClassFactory.CreateClassWithNestedClass();
        
        var serialized = classWithNestedClass.Serialize();
        var deserialized = ClassWithNestedClass.Deserialize(ref serialized);
        
        classWithNestedClass.Should().BeEquivalentTo(deserialized);
    }
}