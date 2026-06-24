using FluentAssertions;
using Xunit.Abstractions;

namespace Test;

public class GeneratedSerializerTest
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
        var classWithList = ClassFactory.CreateClassWithList();
        var classWithHashSet = ClassFactory.CreateClassWithHashSet();
        var classWithDictionary = ClassFactory.CreateClassWithDictionary();
        
        var serializedList = classWithList.Serialize();
        var serializedHashSet = classWithHashSet.Serialize();
        var serializedDictionary = classWithDictionary.Serialize();
        
        var deserializedList = ClassWithList.Deserialize(ref serializedList);
        var deserializedHashSet = ClassWithHashSet.Deserialize(ref serializedHashSet);
        var deserializedDictionary = ClassWithDictionary.Deserialize(ref serializedDictionary);

        classWithList.Should().BeEquivalentTo(deserializedList);
        classWithHashSet.Should().BeEquivalentTo(deserializedHashSet);
        classWithDictionary.Should().BeEquivalentTo(deserializedDictionary);
 
        //TODO: попробовать другие коллекции помимо List, попробовать не примитвные объекты в качестве элементов коллекции
        //TODO: в частности интересно что будет с object
    }
}