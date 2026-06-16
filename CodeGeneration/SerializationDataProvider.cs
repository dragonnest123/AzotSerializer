using Microsoft.CodeAnalysis;

namespace AzotSerializer;

public static class SerializationDataProvider
{
    public static ISymbol[] GetTargetSerializationMembers(INamedTypeSymbol namedType)
    {
        return namedType
            .GetMembers()
            .Where(symbol =>
            {
                if (symbol is IPropertySymbol prop)
                    return !symbol.IsImplicitlyDeclared
                           && symbol.DeclaredAccessibility == Accessibility.Public
                           && prop.SetMethod is not null;
    
                if (symbol is IFieldSymbol field)
                    return !symbol.IsImplicitlyDeclared
                           && symbol.DeclaredAccessibility == Accessibility.Public
                           && !field.IsReadOnly;
    
                return false;
            })
            .ToArray();
    }
}