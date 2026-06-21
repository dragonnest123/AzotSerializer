using System.Runtime.CompilerServices;
using FluentAssertions;
using Serialization.RuntimeSerializers;
using Serialization.RuntimeSerializers.StructSerialization;

namespace Test.RuntimeSerializers;

public class StructSerializerTest
{
    [Fact]
    public void Serialize_UnmanagedStruct()
    {
        var unmanagedStruct = StructsFactory.CreateUnmanagedStruct();
        
        var serialized = StructSerializer.Serialize(ref unmanagedStruct);
        var deserialized = StructSerializer.Deserialize<UnmanagedStruct>(serialized.ToArray());
        
        deserialized.Should().BeEquivalentTo(unmanagedStruct);
    }
    
    [Fact]
    public void Serialize_Into_Buffer_UnmanagedStruct()
    {
        var original = StructsFactory.CreateUnmanagedStruct();
        
        var buffer = new byte[Unsafe.SizeOf<UnmanagedStruct>()];
        
        StructSerializer.Serialize(buffer, 0, ref original);
        var deserializedBuffer = StructSerializer.Deserialize<UnmanagedStruct>(buffer);
        
        deserializedBuffer.Should().BeEquivalentTo(original);
    }
    
    [Fact]
    public void Serialize_Into_Buffer_WithOffset_DoesNotCorruptPrecedingBytes()
    {
        var unmanagedStruct = StructsFactory.CreateUnmanagedStruct();
        byte bufferData = 210;
        var offset = 4;
        var structSize = Unsafe.SizeOf<UnmanagedStruct>();

        var buffer = new byte[offset + structSize];
        buffer[0] = bufferData; 

        buffer[0].Should().Be(bufferData);
        
        StructSerializer.Serialize(buffer, offset, ref unmanagedStruct);
        var slice = buffer.AsSpan(offset, structSize).ToArray();
        var deserialized = StructSerializer.Deserialize<UnmanagedStruct>(slice);
        
        deserialized.Should().BeEquivalentTo(unmanagedStruct);
    }
    
    [Fact]
    public void Serialize_StructWithRefTypes()
    {
        var original = StructsFactory.CreateStructWithRefTypes();
        
        var serialized = StructSerializer.Serialize(ref original);
        var deserialized = StructSerializer.Deserialize<StructWithRefTypes>(serialized.ToArray());

        deserialized.Should().BeEquivalentTo(original,
            options => options.ComparingByMembers<StructWithRefTypes>());
    }
    
    [Fact]
    public void Serialize_StructWithRefTypes_NullableStructField_HasValue()
    {
        var original = StructsFactory.CreateStructWithRefTypes(
            unmanagedStruct: new UnmanagedStruct { Id = 1, Age = 30, DateOfBirth = DateTime.UtcNow });

        var serialized = StructSerializer.Serialize(ref original);
        var deserialized = StructSerializer.Deserialize<StructWithRefTypes>(serialized.ToArray());

        deserialized.BaseData.Should().NotBeNull();
        deserialized.BaseData.Should().BeEquivalentTo(original.BaseData);
    }
    
    [Fact]
    public void Serialize_StructWithRefTypes_NullableStructField_IsNull()
    {
        var original = new StructWithRefTypes
        {
            BaseData = null,
            InternalData = new StructWithRefTypes.InternalClass { Name = "Test" }
        };

        var serialized = StructSerializer.Serialize(ref original);
        var deserialized = StructSerializer.Deserialize<StructWithRefTypes>(serialized.ToArray());

        deserialized.BaseData.Should().BeNull();
    }
}