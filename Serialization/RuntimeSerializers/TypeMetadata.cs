using System.Reflection;
using System.Runtime.CompilerServices;

namespace Serialization.RuntimeSerializers;

internal static class TypeMetadata
{
    private static readonly Dictionary<Type, MemberAccessor[]> _classMembers = [];
    private static readonly Dictionary<Type, StructData> _structs = [];
    
    public static MemberAccessor[] GetMembers<T>()
        => GetMembers(typeof(T));
    
    public static MemberAccessor[] GetMembers(Type type)
    {
        if (_classMembers.TryGetValue(type, out var accessors))
            return accessors;
        
        var props = type
            .GetProperties(Consts.BindingFlag)
            .Where(p => p.CanRead && p.CanWrite)
            .Select(MemberAccessor.Create)
            .ToArray();
        
        var fields = type
            .GetFields(Consts.BindingFlag)
            .Where(f => 
                !f.IsInitOnly 
                && !f.IsLiteral
                && f.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
            .Select(MemberAccessor.Create);
        
        var result = props.Concat(fields).ToArray();

        _classMembers[type] = result;
        return result;
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

    public static bool IsUnmanaged<T>()
    {
        if (!typeof(T).IsValueType)
            return false;
        
        if (_structs.TryGetValue(typeof(T), out var data) && data.IsUnmanaged.HasValue)
            return data.IsUnmanaged.Value;
        
        if (data is null)
            _structs[typeof(T)] = new StructData();
        
        var result = !RuntimeHelpers.IsReferenceOrContainsReferences<T>();
        _structs[typeof(T)].IsUnmanaged = result;
        
        return result;
    }

    public static bool IsUnmanaged(Type type)
    {
        if (!type.IsValueType)
            return false;

        if (_structs.TryGetValue(type, out var data) && data.IsUnmanaged.HasValue)
            return data.IsUnmanaged.Value;

        if (data is null)
            _structs[type] = new StructData();

        if (GetMembers(type).Any(member => !IsUnmanaged(member.Type)))
        {
            _structs[type].IsUnmanaged = false;
            return false;
        }

        _structs[type].IsUnmanaged = true;
        return true;
    }
}