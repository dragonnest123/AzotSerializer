using Microsoft.CodeAnalysis;

namespace AzotSerializer.Utils;

internal static class SerializationDataProvider
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
                           && prop.SetMethod is not null
                           && !HasIgnoreAttribute(prop.SetMethod);
    
                if (symbol is IFieldSymbol field)
                    return !symbol.IsImplicitlyDeclared
                           && symbol.DeclaredAccessibility == Accessibility.Public
                           && !field.IsReadOnly
                           && !HasIgnoreAttribute(field);
    
                return false;
            })
            .ToArray();
    }
    
    private static bool HasIgnoreAttribute(ISymbol member)
        => member.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == GeneratorRegister.IgnoreSerializationAttributeName);
}