using FluentAssertions;

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
    public void Serialize_ClassWithEnum()
    {
        var obj = ClassFactory.CreateClassWithEnum();
    
        var serialized = obj.Serialize();
        var deserialized = ClassWithEnum.Deserialize(ref serialized);
    
        deserialized.Should().BeEquivalentTo(obj);
    }
    
    [Fact]
    public void Serialize_ClassWithArray()
    {
        var original = ClassFactory.CreateClassWithArray();
        var serialized = original.Serialize();
        var deserialized = ClassWithArray.Deserialize(ref serialized);
        
        deserialized.Should().BeEquivalentTo(original);
        
        var classWithJaggedArray = ClassFactory.CreateClassWithJaggedArray();
        var serializedJaggedArray = classWithJaggedArray.Serialize();
        var deserializedJaggedArray = ClassWithJaggedArray.Deserialize(ref serializedJaggedArray);
        
        classWithJaggedArray.Should().BeEquivalentTo(deserializedJaggedArray);  
    }

    [Fact]
    public void Serialize_ClassWithArray_EmptyArray()
    {
        var original = ClassFactory.CreateClassWithArray([]);
        
        var serialized = original.Serialize();
        var deserialized = ClassWithArray.Deserialize(ref serialized);
        
        deserialized.Array.Should().BeEmpty();
    }

    [Fact]
    public void Serialize_ClassWithArray_NullArray()
    {
        var original = new ClassWithArray { Array = null };
        
        var serialized = original.Serialize();
        var deserialized = ClassWithArray.Deserialize(ref serialized);
        
        deserialized.Array.Should().BeNull();
    }
    
    [Fact]
    public void Serialize_ClassWithCollection()
    {
        var classWithList = ClassFactory.CreateClassWithList();
        var serializedList = classWithList.Serialize();
        var deserializedList = ClassWithList.Deserialize(ref serializedList);
        
        classWithList.Should().BeEquivalentTo(deserializedList);
        
        var classWithHashSet = ClassFactory.CreateClassWithHashSet();
        var serializedHashSet = classWithHashSet.Serialize();
        var deserializedHashSet = ClassWithHashSet.Deserialize(ref serializedHashSet);
        
        classWithHashSet.Should().BeEquivalentTo(deserializedHashSet);
        
        var classWithDictionary = ClassFactory.CreateClassWithDictionary();
        var serializedDictionary = classWithDictionary.Serialize();
        var deserializedDictionary = ClassWithDictionary.Deserialize(ref serializedDictionary);
        
        classWithDictionary.Should().BeEquivalentTo(deserializedDictionary);
        
        var classWithListObjects = ClassFactory.CreateClassWithListObjects();
        var serializedListObjects = classWithListObjects.Serialize();
        var deserializedListObjects = ClassWithListObjects.Deserialize(ref serializedListObjects);
        
        classWithListObjects.Should().BeEquivalentTo(deserializedListObjects);
        
        var classWithNestedCollection = ClassFactory.CreateClassWithNestedCollection();

        var serializedClassWithNestedCollection = classWithNestedCollection.Serialize();
        var deserializedClassWithNestedCollection = ClassWithNestedCollection.Deserialize(ref serializedClassWithNestedCollection);

        deserializedClassWithNestedCollection.Should().BeEquivalentTo(classWithNestedCollection);
    }
}