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
    
    public static ClassWithEnum CreateClassWithEnum()
        => new ClassWithEnum
        {
            Day = DayOfWeek.Wednesday,
            NullableDay = DayOfWeek.Friday
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

    public static ClassWithArray CreateClassWithArray(int[]? array = null)
        => new ClassWithArray
        {
            Array = array ?? [123, 4124, 5142, 14124, 2, 2, 2, 3414, 1, 12]
        };
    
    public static ClassWithJaggedArray CreateClassWithJaggedArray()
        => new ClassWithJaggedArray
        {
            JaggedArray = 
            [
                [new NestedClass { Value = 1 }, new NestedClass { Value = 2 }],
                [3, 4, 5, 6, 512413, 351341, 45132, 42514],
                ["asdasd", "dgfadkasd", "dfkgdkgfdkfg", "adsaksdkasd"],
                [CreateClassWithArray(), CreateClassWithList(), CreateClassWithDictionary()]
            ]
        };

    public static ClassWithList CreateClassWithList(List<int>? list = null)
        => new ClassWithList
        {
            List = list ?? [234, 123, 7, 35235, 1236, 24034, 23124, 6234, 4581832, 1281300]
        };

    public static ClassWithHashSet CreateClassWithHashSet(HashSet<SimpleClass>? set = null)
        => new ClassWithHashSet
        {
            HashSet = set ?? [CreateSimpleClass(), CreateSimpleClass(), CreateSimpleClass()]
        };

    public static ClassWithDictionary CreateClassWithDictionary(Dictionary<int, string>? dictionary = null)
        => new ClassWithDictionary
        {
            Dictionary = dictionary ?? new Dictionary<int, string>
            {
                [1] = "one",
                [2] = "two",
                [3] = "three",
                [100] = "hundred"
            }
        };

    public static ClassWithListObjects CreateClassWithListObjects(List<object>? list = null)
        => new ClassWithListObjects
        {
            ListObjects = list ?? [CreateSimpleClass(), CreateClassWithNestedObjects(), 123, "testString5132"]
        };

    public static ComplexClass CreateComplexClass()
        => new ComplexClass
        {
            Int = int.MaxValue,
            UInt = uint.MaxValue,
            Long = long.MaxValue,
            ULong = ulong.MaxValue,
            Short = short.MaxValue,
            UShort = ushort.MaxValue,
            Byte = byte.MaxValue,
            SByte = sbyte.MaxValue,
            Float = float.MaxValue,
            Double = double.MaxValue,
            Decimal = decimal.MaxValue,
            Bool = true,
            Char = 'S',
            String = "hello world",
            DateTime = DateTime.MaxValue,
            TimeSpan = TimeSpan.MaxValue,
            Enum = DayOfWeek.Sunday,
            NullableEnum = null,
            NullableInt = 42,
            NullableString = null,
            NestedClass = CreateSimpleClass(),
            NestedStruct = new NestedStruct { Value = 999, Nested = new NestedClass { Value = 1 } },
            List = [1, 2, 3, 4, 5],
            Array = [10, 20, 30],
            Dictionary = new Dictionary<int, string> { [1] = "one", [2] = "two" },
            ValueTuple = (99, "tuple")
        };
}