using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Serialization.RuntimeSerialization;

internal static class TypeDataProvider
{
    private static readonly Dictionary<Type, ObjectData> _objects = [];
    private static readonly Dictionary<Type, StructData> _structs = [];
    
    public static MemberAccessor[] GetMembers<T>()
        => GetMembers(typeof(T));
    
    public static MemberAccessor[] GetMembers(Type type)
    {
        if (_objects.TryGetValue(type, out var objectData) && objectData.Accessors is not null)
            return objectData.Accessors;
        
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

        if (objectData is null)
            _objects[type] = new ObjectData { Accessors = result };
        else
            _objects[type].Accessors = result;
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
        
        var result = !RuntimeHelpers.IsReferenceOrContainsReferences<T>();
        
        if (data is null)
            _structs[typeof(T)] = new StructData { IsUnmanaged = result };
        else
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
            _structs[type] = new StructData { IsUnmanaged = false };

        if (GetMembers(type).Any(member => !IsUnmanaged(member.Type)))
            return false;

        _structs[type].IsUnmanaged = true;
        return true;
    }
    
    public static bool CanBeNull(Type type)
        => !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    
    public static bool IsCollection<T>([NotNullWhen(true)] out Type? collectionInterface)
        => IsCollection(typeof(T), out collectionInterface);

    public static bool IsCollection(Type type, [NotNullWhen(true)] out Type? collectionInterface)
    {
        if (_objects.TryGetValue(type, out var objectData) && objectData.CollectionInterfaceType is not null)
        {
            collectionInterface = objectData.CollectionInterfaceType;
            return true;
        }
        
        if (objectData is null)
            _objects[type] = new ObjectData();

        collectionInterface = type
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
        
        if (collectionInterface is null)
            return false;
        
        _objects[type].CollectionInterfaceType = collectionInterface;
        return true;
    }
}