using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AzotSerializer;

public static class SyntaxValidator
{
    public static bool SyntaxIsValid(
        TypeDeclarationSyntax typeSyntax,
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
}