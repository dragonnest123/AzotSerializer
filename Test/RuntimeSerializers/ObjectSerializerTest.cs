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
    public void Serialize_ClassWithEnum()
    {
        var obj = ClassFactory.CreateClassWithEnum();
    
        var serialized = ObjectSerializer.Serialize(obj);
        var deserialized = ObjectSerializer.Deserialize<ClassWithEnum>(serialized.ToArray());
    
        deserialized.Should().BeEquivalentTo(obj);
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
    public void Serialize_ClassWithCyclicReference()
    {
        var nestedClass = new NestedClass { Value = 123124, Nested = new NestedClass { Value = 456 } };
        nestedClass.Nested.Nested = nestedClass;
        
        var exception = Record.Exception(() =>
        {
            var span = ObjectSerializer.Serialize(nestedClass);
            _ = span.ToArray();
        });
        
        exception.Should().BeNull();
    }
    
    [Fact]
    public void Serialize_ClassWithArray()
    {
        var classWithArray = ClassFactory.CreateClassWithArray();
        var serializedArray = ObjectSerializer.Serialize(classWithArray);
        var deserializedArray = ObjectSerializer.Deserialize<ClassWithArray>(serializedArray.ToArray());
        
        classWithArray.Should().BeEquivalentTo(deserializedArray);   
        
        var classWithJaggedArray = ClassFactory.CreateClassWithJaggedArray();
        var serializedJaggedArray = ObjectSerializer.Serialize(classWithJaggedArray);
        var deserializedJaggedArray = ObjectSerializer.Deserialize<ClassWithJaggedArray>(serializedJaggedArray.ToArray());
        
        classWithJaggedArray.Should().BeEquivalentTo(deserializedJaggedArray);  
    }
    
    [Fact]
    public void Serialize_EmptyArray()
    {
        var obj = ClassFactory.CreateClassWithArray([]);
        var serialized = ObjectSerializer.Serialize(obj);
        var deserialized = ObjectSerializer.Deserialize<ClassWithArray>(serialized.ToArray());
        deserialized.Should().BeEquivalentTo(obj);
    }

    [Fact]
    public void Serialize_NullArray()
    {
        var obj = new ClassWithArray { Array = null };
        var serialized = ObjectSerializer.Serialize(obj);
        var deserialized = ObjectSerializer.Deserialize<ClassWithArray>(serialized.ToArray());
        deserialized.Should().BeEquivalentTo(obj);
    }
    
    [Fact]
    public void Serialize_ArrayWithNullElements()
    {
        var obj = new int?[] { null, 14123, null };
        var serialized = ObjectSerializer.Serialize(obj);
        var deserialized = ObjectSerializer.Deserialize<int?[]>(serialized.ToArray());
        deserialized.Should().BeEquivalentTo(obj);
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
        
        var classWithListObjects = ClassFactory.CreateClassWithListObjects();
        var serializedListObjects = ObjectSerializer.Serialize(classWithListObjects);
        var deserializedListObjects = ObjectSerializer.Deserialize<ClassWithListObjects>(serializedListObjects.ToArray());
        
        classWithListObjects.Should().BeEquivalentTo(deserializedListObjects);
    }
    
    [Fact]
    public void Serialize_EmptyCollection()
    {
        var obj = ClassFactory.CreateClassWithList([]);
        var serialized = ObjectSerializer.Serialize(obj);
        var deserialized = ObjectSerializer.Deserialize<ClassWithList>(serialized.ToArray());
        deserialized.Should().BeEquivalentTo(obj);
    }

    [Fact]
    public void Serialize_NullCollection()
    {
        var obj = new ClassWithList { List = null };
        var serialized = ObjectSerializer.Serialize(obj);
        var deserialized = ObjectSerializer.Deserialize<ClassWithList>(serialized.ToArray());
        deserialized.Should().BeEquivalentTo(obj);
    }

    [Fact]
    public void Serialize_CollectionWithNullElements()
    {
        var obj = new List<string?> { null, "hello", null };
        var serialized = ObjectSerializer.Serialize(obj);
        var deserialized = ObjectSerializer.Deserialize<List<string?>>(serialized.ToArray());
        deserialized.Should().BeEquivalentTo(obj);
    }
    
    [Fact]
    public void Serialize_ComplexClass()
    {
        var obj = ClassFactory.CreateComplexClass();
    
        var serialized = ObjectSerializer.Serialize(obj);
        var deserialized = ObjectSerializer.Deserialize<ComplexClass>(serialized.ToArray());
    
        deserialized.Should().BeEquivalentTo(obj, 
            options => options.ComparingByMembers<NestedStruct>());
    }
}