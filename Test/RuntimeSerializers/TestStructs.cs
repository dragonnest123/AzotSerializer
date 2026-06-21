namespace Test.RuntimeSerializers;

public struct UnmanagedStruct
{
    public int? Id { get; set; }
    public int Age { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public struct StructWithRefTypes
{
    public class InternalClass
    {
        public string Name { get; set; }
    }
    
    public UnmanagedStruct? BaseData { get; set; }
    public InternalClass InternalData { get; set; }
}