using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Serialization.RuntimeSerializers;

internal static class TypeMetadata
{
    private static readonly Dictionary<Type, PropertyAccessor[]> _classMembers = [];
    private static readonly Dictionary<Type, StructData> _structs = [];

    public static PropertyAccessor[] GetProperties<T>()
        => GetProperties(typeof(T));
    
    public static PropertyAccessor[] GetProperties(Type type)
    {
        if (_classMembers.TryGetValue(type, out var accessors))
            return accessors;
        
        var props = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Select(PropertyAccessor.Create)
            .ToArray();
        
        _classMembers[type] = props;
        return props;
    }
    
    public static int GetStructSize(Type type)
    {
        if (_structs.TryGetValue(type, out var data) && data.Size.HasValue)
            return data.Size.Value;
        
        var method = typeof(Unsafe).GetMethod("SizeOf") 
                     ?? throw new InvalidOperationException("Unsafe doesn't have SizeOf method");
        
        var result = method.MakeGenericMethod(type).Invoke(null, null)
                     ?? throw new Exception("Null struct size");

        if (data is not null)
            _structs[type].Size = (int)result;
        else
            _structs[type] = new StructData { Size = (int)result };
        
        return (int)result;
    }

    public static bool IsBlittable(Type type)
    {
        if (_structs.TryGetValue(type, out var data) && data.IsBlittable.HasValue)
            return data.IsBlittable.Value;
        
        if (data is null)
            _structs[type] = new StructData();
        
        if (type.IsArray)
        {
            var elem = type.GetElementType();
            return elem is not null && elem.IsValueType && IsBlittable(elem);
        }

        try
        {
            object? instance = Activator.CreateInstance(type);
            GCHandle.Alloc(instance, GCHandleType.Pinned).Free();
            
            _structs[type].IsBlittable = true;
        
            return true;
        }
        catch (ArgumentException)
        {
            _structs[type].IsBlittable = false;
            
            return false;
        }
    }
}