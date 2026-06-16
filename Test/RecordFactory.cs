namespace Test;

public static class ClassFactory
{
    public static SimpleClass CreateSimpleClass(
        int id = 0, 
        string name = "QWERTYUIOPASDFGHJKLZXCVBNM", 
        int age = 100)
        => new SimpleClass
        {
            Id = id,
            Name = name,
            Age = age
        };

    public static ClassWithNestedObjects CreateClassWithNestedObjects(
        NestedClass? nestedClass = null,
        NestedStruct? nestedStruct = null,
        NestedRecord? nestedRecord = null,
        NestedRecordStruct? nestedRecordStruct = null)
        => new ClassWithNestedObjects()
        {
            Class = nestedClass ?? new NestedClass { Value = 123124, Nested = new NestedClass { Value = 456 } },
            Struct = nestedStruct ?? new NestedStruct { Value = 456, Nested = new NestedClass { Value = 5123 } },
            Record = nestedRecord ?? new NestedRecord { Value = 456, Nested = new NestedStruct { Value = 1} },
            RecordStruct = nestedRecordStruct ?? new NestedRecordStruct { Value = 456, Nested = new NestedStruct { Value = 3} }
        };

    public static ClassWithCollection CreateClassWithCollection(
        List<int>? list = null, 
        Dictionary<int, string>? dictionary = null)
        => new ClassWithCollection
        {
            ListCollection = list ?? [234, 123, 7, 35235, 1236, 24034, 23124, 6234, 4581832, 1281300],
            DictionaryCollection = dictionary ?? new Dictionary<int, string>
            {
                [1] = "one",
                [2] = "two",
                [3] = "three",
                [100] = "hundred"
            }
        };

    public static ComplexClass CreateComplexClass(
        int id = 0, 
        string? name = "ABC", 
        int age = 100,
        bool boolean = false,
        ComplexClass.NestedEnum? e = ComplexClass.NestedEnum.EnumValue1,
        NestedClass? nestedClass = null,
        NestedStruct? nestedStruct = null)
    {
        return new ComplexClass
        {
            Id = id,
            Name = name,
            Age = age,
            Boolean = boolean,
            Enum = e,
            Class = nestedClass ?? new NestedClass { Value = 123124 },
            Struct = nestedStruct ??  new NestedStruct { Value = 5425132 }
        };
    }
}