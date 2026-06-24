using FluentAssertions;
using Serialization.RuntimeSerialization.Serializers;

namespace Test.RuntimeSerializers;

public class ObjectSerializerTest
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

        deserialized.Should().BeEquivalentTo(c,
            options => options.ComparingByMembers<NestedStruct>());
    }
    
    [Fact]
    public void Serialize_ClassWithCollection()
    {
        var classWithList = ClassFactory.CreateClassWithList();
        var serializedList = ObjectSerializer.Serialize(classWithList);
        var deserializedList = ObjectSerializer.Deserialize<ClassWithList>(serializedList.ToArray());
        
        classWithList.Should().BeEquivalentTo(deserializedList);
        
        var classWithHashSet = ClassFactory.CreateClassWithHashSet();
        var serializedHashSet = ObjectSerializer.Serialize(classWithHashSet);
        var deserializedHashSet = ObjectSerializer.Deserialize<ClassWithHashSet>(serializedHashSet.ToArray());
        
        classWithHashSet.Should().BeEquivalentTo(deserializedHashSet);
        
        var classWithDictionary = ClassFactory.CreateClassWithDictionary();
        var serializedDictionary = ObjectSerializer.Serialize(classWithDictionary);
        var deserializedDictionary = ObjectSerializer.Deserialize<ClassWithDictionary>(serializedDictionary.ToArray());
        
        classWithDictionary.Should().BeEquivalentTo(deserializedDictionary);
    }
}