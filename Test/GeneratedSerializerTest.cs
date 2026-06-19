using FluentAssertions;
using Xunit.Abstractions;

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
    public void Serialize_ClassWithNestedObjects()
    {
        var classWithNestedClass = ClassFactory.CreateClassWithNestedObjects();

        var serialized = classWithNestedClass.Serialize();
        var deserialized = ClassWithNestedObjects.Deserialize(ref serialized);

        classWithNestedClass.Should().BeEquivalentTo(deserialized, options => 
            options.ComparingByMembers<NestedStruct>());
    }
    
    [Fact]
    public void Serialize_ClassWithCollection()
    {
        var classWithCollection = ClassFactory.CreateClassWithCollection();
        
        var serialized = classWithCollection.Serialize();
        var deserialized = ClassWithCollection.Deserialize(ref serialized);
        
        classWithCollection.Should().BeEquivalentTo(deserialized);
 
        //TODO: попробовать другие коллекции помимо List, попробовать не примитвные объекты в качестве элементов коллекции
        //TODO: в частности интересно что будет с object
    }
}