using AzotSerializer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AzotSerializer;

internal static class SyntaxValidator
{
    public static bool IsTypeSerializable(
        TypeDeclarationSyntax typeSyntax,
        ISymbol[] typeMembers,
        SourceProductionContext productionContext)
    {
        if (!IsPartial(typeSyntax))
        {
            productionContext.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MustBePartial, 
                typeSyntax.Identifier.GetLocation(),
                typeSyntax.Identifier.Text));

            return false;
        }
        
        var nonPartial = GetFirstNonPartial(typeSyntax);
        if (nonPartial is not null)
        {
            productionContext.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NestedContainingTypesMustBePartial,
                nonPartial.Identifier.GetLocation(),
                nonPartial.Identifier.Text));
            
            return false;
        }
        
        var nonSupported = GetFirstNonSupportedTypeName(typeMembers);
        if (nonSupported is not null)
        {
            productionContext.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NotSupportedType,
                typeSyntax.Identifier.GetLocation(),
                nonSupported));
        }
        
        return true;
    }

    private static TypeDeclarationSyntax? GetFirstNonPartial(TypeDeclarationSyntax typeSyntax)
    {
        while (true)
        {
            if (!IsPartial(typeSyntax))
                return typeSyntax;

            if (typeSyntax.Parent is null || typeSyntax.Parent is not TypeDeclarationSyntax parentSyntax)
                return null;

            typeSyntax = parentSyntax;
        }
    }

    private static bool IsPartial(TypeDeclarationSyntax typeSyntax)
        => typeSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword));

    private static string? GetFirstNonSupportedTypeName(ISymbol[] typeMembers)
    {
        foreach (var member in typeMembers)
        {
            var type = member
                .GetPropertyOrFieldType()?
                .GetNullableValueUnderlyingType();
            
            if (type is not null && !IsSupportedType(type))
                return member.ToDisplayString();
        }

        return null;
    }

    private static bool IsSupportedType(ITypeSymbol typeSymbol)
    {
        if (NativelySupportedTypes.IsSupported(typeSymbol))
            return true;
        
        if (typeSymbol is not INamedTypeSymbol namedType) 
            return false;
        
        return HasSerializationAttribute(namedType); 
    }

    private static bool HasSerializationAttribute(INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == GeneratorRegister.SerializationAttributeName);
}