using Serialization;

namespace Test;

[ByteSerializable]
public partial class SimpleClass
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? Age { get; set; }
}

[ByteSerializable]
public partial class ClassWithEnum
{
    public DayOfWeek Day { get; set; }
    public DayOfWeek? NullableDay { get; set; }
}

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
public partial class ClassWithNestedObjects
{
    public NestedClass? Class { get; set; }
    public NestedStruct? Struct { get; set; }
    public NestedRecord? Record { get; set; }
    public NestedRecordStruct? RecordStruct { get; set; }
}

[ByteSerializable]
public partial class ClassWithArray
{
    public int[]? Array { get; set; }
}

[ByteSerializable]
public partial class ClassWithJaggedArray
{
    public object[][]? JaggedArray { get; set; }
}

[ByteSerializable]
public partial class ClassWithList
{
    public List<int>? List { get; set; }
}

[ByteSerializable]
public partial class ClassWithNestedCollection
{
    public List<List<int>>? ListOfLists { get; set; }
}

[ByteSerializable]
public partial class ClassWithHashSet
{
    public HashSet<SimpleClass>? HashSet { get; set; }
}

[ByteSerializable]
public partial class ClassWithDictionary
{
    public Dictionary<int, string>? Dictionary { get; set; }
}

[ByteSerializable]
public partial class ClassWithListObjects
{
    public List<object>? ListObjects { get; set; }
}

public partial class ComplexClass
{
    public int Int { get; set; }
    public uint UInt { get; set; }
    public long Long { get; set; }
    public ulong ULong { get; set; }
    public short Short { get; set; }
    public ushort UShort { get; set; }
    public byte Byte { get; set; }
    public sbyte SByte { get; set; }
    public float Float { get; set; }
    public double Double { get; set; }
    public decimal Decimal { get; set; }
    public bool Bool { get; set; }
    public char Char { get; set; }
    public string? String { get; set; }
    public DateTime DateTime { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public DayOfWeek Enum { get; set; }
    public DayOfWeek? NullableEnum { get; set; }
    public int? NullableInt { get; set; }
    public string? NullableString { get; set; }
    public SimpleClass? NestedClass { get; set; }
    public NestedStruct NestedStruct { get; set; }
    public List<int>? List { get; set; }
    public int[]? Array { get; set; }
    public Dictionary<int, string>? Dictionary { get; set; }
    public (int, string) ValueTuple { get; set; }
}