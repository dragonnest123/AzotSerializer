namespace Test.RuntimeSerializers;

public static class StructsFactory
{
    public static UnmanagedStruct CreateUnmanagedStruct(
        int id = 0, 
        int age = 100,
        DateTime? date = null)
        => new UnmanagedStruct
        {
            Id = id,
            Age = age,
            DateOfBirth = date ?? new DateTime(2001, 10, 3)
        };
    
    public static StructWithRefTypes CreateStructWithRefTypes(
        UnmanagedStruct? unmanagedStruct = null,
        StructWithRefTypes.InternalClass? internalClass = null)
        => new StructWithRefTypes
        {
            BaseData = unmanagedStruct ?? CreateUnmanagedStruct(),
            InternalData = internalClass ?? new StructWithRefTypes.InternalClass { Name = "Pider" }
        };
}