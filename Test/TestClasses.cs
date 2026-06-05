using Serialization;

namespace Test;

[ByteSerializable]
public partial class NestedClass
{
    public int Value { get; set; }
    public NestedClass? Nested { get; set; }
}

public partial struct NestedStruct
{
    public int Value { get; set; }
}

[ByteSerializable]
public partial class SimpleClass
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
}

[ByteSerializable]
public partial class ClassWithNestedClass
{
    public NestedClass? NestedClass { get; set; }
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