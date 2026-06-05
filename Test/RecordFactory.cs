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

    public static ClassWithNestedClass CreateClassWithNestedClass(NestedClass? nestedClass = null)
        => new ClassWithNestedClass
        {
            NestedClass = nestedClass ??  new NestedClass { Value = 123124, Nested = new NestedClass { Value = 456 } },
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