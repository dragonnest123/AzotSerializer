using FluentAssertions;
using Serialization.RuntimeSerialization;

namespace Test.RuntimeSerializers;

public class TypeDataProviderTest
{
    private class SimpleClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Age;
    }
    
    private class ClassWithReadOnlyProperty
    {
        public int Id { get; set; }
        public int ReadOnly => 5;
    }
    
    private class ClassWithWriteOnlyProperty
    {
        public int Id { get; set; }
        public int WriteOnly { set { } }
    }
    
    private class EmptyClass { }
    
    private struct Point
    {
        public int X;
        public int Y;
    }
    
    private struct StructWithString
    {
        public int X;
        public string Name;
    }
    
    private struct EmptyStruct { }
    
    private struct OuterStruct
    {
        public Point Inner;
        public int Z;
    }
    
    private struct OuterStructWithManagedInner
    {
        public StructWithString Inner;
        public int Z;
    }
    
    private struct StructWithArrayField
    {
        public int[] Items;
    }
    
    public class GetMembersTest
    {
        [Fact]
        public void GetMembers_Returns_ReadWriteProperties()
        {
            var props = TypeDataProvider.GetMembers(typeof(SimpleClass));

            props.Should().HaveCount(3);
            props.Select(p => p.Type).Should().BeEquivalentTo([typeof(int), typeof(string), typeof(int)]);
        }
    
        [Fact]
        public void GetMembers_Excludes_ReadOnlyProperty()
        {
            var props = TypeDataProvider.GetMembers(typeof(ClassWithReadOnlyProperty));

            props.Should().HaveCount(1);
        }
    
        [Fact]
        public void GetMembers_Excludes_WriteOnlyProperty()
        {
            var props = TypeDataProvider.GetMembers(typeof(ClassWithWriteOnlyProperty));

            props.Should().HaveCount(1);
        }
    
        [Fact]
        public void GetMembers_Returns_SameArrayInstance_OnSecondCall()
        {
            var first = TypeDataProvider.GetMembers(typeof(SimpleClass));
            var second = TypeDataProvider.GetMembers(typeof(SimpleClass));

            first.Should().BeSameAs(second);
        }
    
        [Fact]
        public void GetMembers_Returns_EmptyArray_ForClass_WithoutProperties()
        {
            var props = TypeDataProvider.GetMembers(typeof(EmptyClass));

            props.Should().BeEmpty();
        }   
    }
    
    public class GetStructSizeTest
    {
        [Fact]
        public void GetStructSize_ReturnsFour_ForInt()
        {
            var size = TypeDataProvider.GetStructSize(typeof(int));

            size.Should().Be(4);
        }
        
        [Fact]
        public void GetStructSize_Returns_CorrectSize_ForSimpleStruct()
        {
            var size = TypeDataProvider.GetStructSize(typeof(Point));

            size.Should().Be(8);
        }
        
        [Fact]
        public void GetStructSize_Returns_SameValue_OnSecondCall()
        {
            var first = TypeDataProvider.GetStructSize(typeof(Point));
            var second = TypeDataProvider.GetStructSize(typeof(Point));

            first.Should().Be(second);
        }
        
        [Fact]
        public void GetStructSize_ThrowsOrReturnsMinimumSize_ForEmptyStruct()
        {
            // CLR гарантирует 1 байт для пустой структуры
            var size = TypeDataProvider.GetStructSize(typeof(EmptyStruct));

            size.Should().Be(1);
        }
    }
    
    public class IsUnmanagedGenericTypeTest
    {
        [Fact]
        public void IsUnmanagedGeneric_Returns_True_ForInt()
        {
            TypeDataProvider.IsUnmanaged<int>().Should().BeTrue();
        }

        [Fact]
        public void IsUnmanagedGeneric_Returns_True_For_UnmanagedStruct()
        {
            TypeDataProvider.IsUnmanaged<Point>().Should().BeTrue();
        }

        [Fact]
        public void IsUnmanagedGeneric_Returns_False_For_StructWithManagedField()
        {
            TypeDataProvider.IsUnmanaged<StructWithString>().Should().BeFalse();
        }

        [Fact]
        public void IsUnmanagedGeneric_ReturnsFalse_ForClass()
        {
            TypeDataProvider.IsUnmanaged<SimpleClass>().Should().BeFalse();
        }

        [Fact]
        public void IsUnmanagedGeneric_CachesResult()
        {
            var first = TypeDataProvider.IsUnmanaged<Point>();
            var second = TypeDataProvider.IsUnmanaged<Point>();

            first.Should().Be(second);
        }
    }

    public class IsUnmanagedTypeTest
    {
        [Fact]
        public void IsUnmanaged_Returns_True_For_Primitive()
        {
            TypeDataProvider.IsUnmanaged(typeof(int)).Should().BeTrue();
        }

        [Fact]
        public void IsUnmanaged_Returns_False_For_Class()
        {
            TypeDataProvider.IsUnmanaged(typeof(SimpleClass)).Should().BeFalse();
        }

        [Fact]
        public void IsUnmanaged_Returns_True_For_UnmanagedStruct()
        {
            TypeDataProvider.IsUnmanaged(typeof(Point)).Should().BeTrue();
        }

        [Fact]
        public void IsUnmanaged_Returns_False_For_StructWithManagedField()
        {
            TypeDataProvider.IsUnmanaged(typeof(StructWithString)).Should().BeFalse();
        }

        [Fact]
        public void IsUnmanaged_Returns_True_For_NestedUnmanagedStruct()
        {
            TypeDataProvider.IsUnmanaged(typeof(OuterStruct)).Should().BeTrue();
        }

        [Fact]
        public void IsUnmanaged_Returns_False_For_NestedManagedStruct()
        {
            TypeDataProvider.IsUnmanaged(typeof(OuterStructWithManagedInner)).Should().BeFalse();
        }

        [Fact]
        public void IsUnmanaged_CachesResult()
        {
            var first = TypeDataProvider.IsUnmanaged(typeof(Point));
            var second = TypeDataProvider.IsUnmanaged(typeof(Point));

            first.Should().Be(second);
        }

        [Fact]
        public void IsUnmanaged_Returns_False_For_StructWithArrayField()
        {
            TypeDataProvider.IsUnmanaged(typeof(StructWithArrayField)).Should().BeFalse();
        }
    }
}