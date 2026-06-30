using AzotSerializer.Extensions;
using Microsoft.CodeAnalysis;

namespace AzotSerializer;

public static class NativelySupportedTypes
{
    public static bool IsSupported(ITypeSymbol type)
        => PrimitiveSupportedTypes.IsSupported(type) ||
           CollectionSupportedTypes.IsSupported(type) ||
           WellKnownSupportedTypes.IsSupported(type);
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
        return type.IsEnum() || _primitiveTypes.Contains(type.SpecialType);
    }
}

public static class CollectionSupportedTypes
{
    public const string CollectionInterface = "System.Collections.Generic.ICollection<T>";
    
    private static readonly HashSet<string> _collectionTypes =
    [
        CollectionInterface,
    ];
    
    public static bool IsSupported(ITypeSymbol type)
    {
        return type.AllInterfaces.Any(i => 
            _collectionTypes.Contains(i.OriginalDefinition.ToDisplayString()));
    }
}

public static class WellKnownSupportedTypes
{
    public const string DateTime = "System.DateTime";
    public const string TimeSpan = "System.TimeSpan";
    public const string KeyValuePair = "System.Collections.Generic.KeyValuePair<TKey, TValue>";
    
    private static readonly HashSet<string> _miscellaneousTypes = 
    [
        DateTime,
        TimeSpan,
        KeyValuePair
    ];
    
    public static bool IsSupported(ITypeSymbol type)
    {
        return _miscellaneousTypes.Contains(type.OriginalDefinition.ToDisplayString());
    }
}