using Serialization;

namespace Test;

[ByteSerializable]
public partial class NestedClass
{
    public int Value { get; set; }
    public NestedClass? Nested { get; set; }
}

[ByteSerializable]
public partial struct NestedStruct
{
    public int Value { get; set; }
    public NestedClass? Nested { get; set; }
}

[ByteSerializable]
public partial record NestedRecord
{
    public int Value { get; set; }
    public NestedStruct? Nested { get; set; }
}

[ByteSerializable]
public partial record struct NestedRecordStruct
{
    public int Value { get; set; }
    public NestedStruct? Nested { get; set; }
}

[ByteSerializable]
public partial class SimpleClass
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? Age { get; set; }
}

[ByteSerializable]
public partial class ClassWithNestedObjects
{
    public NestedClass? Class { get; set; }
    public NestedStruct? Struct { get; set; }
    public NestedRecord? Record { get; set; }
    public NestedRecordStruct? RecordStruct { get; set; }
}


public class ComplexClass
{
    public enum NestedEnum
    {
        EnumValue1,
        EnumValue2,
        EnumValue3
    }
    
    public int? Id { get; set; }
    public string? Name { get; set; }
    public int? Age { get; set; }
    public bool? Boolean { get; set; }
    public NestedEnum? Enum { get; set; }
    public NestedClass? Class { get; set; }
    public NestedStruct? Struct { get; set; }
}