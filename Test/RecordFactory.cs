using AzotBase.Common.Serialization.Attributes;

namespace AzotName.Tests.TestUtils.Factories;

public static class RecordFactory
{
    public static SimpleRecord CreateSimpleRecord(int id = 0, string? name = "QWERTYUIOPASDFGHJKLZXCVBNM", int age = 100)
        => new SimpleRecord
        {
            Id = id,
            Name = name,
            Age = age
        };

    public static ComplexRecord CreateComplexRecord(
        int id = 0, 
        string? name = "ABC", 
        int age = 100,
        bool boolean = false,
        ComplexRecord.NestedEnum? e = ComplexRecord.NestedEnum.EnumValue1,
        ComplexRecord.NestedClass? nestedClass = null,
        ComplexRecord.NestedStruct? nestedStruct = null)
    {
        return new ComplexRecord
        {
            Id = id,
            Name = name,
            Age = age,
            Boolean = boolean,
            Enum = e,
            Class = nestedClass ?? new ComplexRecord.NestedClass { Value = 123124 },
            Struct = nestedStruct ??  new ComplexRecord.NestedStruct { Value = 5425132 }
        };
    }
}

public class SimpleRecord
{
    [SerializeOrder(0)] public int Id { get; set; }
    [SerializeOrder(1)] public string? Name { get; set; }
    [SerializeOrder(2)] public int Age { get; set; }
}

public class ComplexRecord
{
    public class NestedClass
    {
        public int Value { get; set; }
        public NestedStruct Struct { get; set; } = new() { Value = 5 };
    }

    public struct NestedStruct
    {
        public int Value { get; set; }
    }

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