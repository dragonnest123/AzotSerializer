using Microsoft.CodeAnalysis;

namespace AzotSerializer;

public static class NativelySupportedTypes
{
    public static bool IsSupported(ITypeSymbol type)
        => PrimitiveSupportedTypes.IsSupported(type) ||
           CollectionSupportedTypes.IsSupported(type) ||
           MiscellaneousSupportedTypes.IsSupported(type);
}

public static class PrimitiveSupportedTypes
{
    private static readonly HashSet<SpecialType> _primitiveTypes =
    [
        SpecialType.System_Boolean,
        SpecialType.System_Byte,
        SpecialType.System_SByte,
        SpecialType.System_Int16,
        SpecialType.System_UInt16,
        SpecialType.System_Int32,
        SpecialType.System_UInt32,
        SpecialType.System_Int64,
        SpecialType.System_UInt64,
        SpecialType.System_Single,
        SpecialType.System_Double,
        SpecialType.System_Decimal,
        SpecialType.System_String,
    ];
    
    public static bool IsSupported(ITypeSymbol type)
    {
        return _primitiveTypes.Contains(type.SpecialType);
    }
}

public static class CollectionSupportedTypes
{
    public static string GenericCollectionInterface => "System.Collections.Generic.ICollection<T>";
    
    private static readonly HashSet<string> _collectionTypes =
    [
        GenericCollectionInterface,
    ];
    
    public static bool IsSupported(ITypeSymbol type)
    {
        return type.AllInterfaces.Any(i => 
            _collectionTypes.Contains(i.OriginalDefinition.ToDisplayString()));
    }
}

public static class MiscellaneousSupportedTypes
{
    private static readonly HashSet<string> _miscellaneousTypes = 
    [
        "System.Collections.Generic.KeyValuePair<TKey, TValue>"
    ];
    
    public static bool IsSupported(ITypeSymbol type)
    {
        return type.AllInterfaces.Any(i => 
            _miscellaneousTypes.Contains(i.OriginalDefinition.ToDisplayString()));
    }
}