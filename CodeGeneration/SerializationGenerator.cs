using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AzotSerializer;

internal static class SerializationGenerator
{
    public static void Generate(
        TypeDeclarationSyntax typeSyntax, 
        Compilation compilation,
        SourceProductionContext productionContext)
    {
        var semanticModel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);

        if (ModelExtensions.GetDeclaredSymbol(semanticModel, typeSyntax) is not INamedTypeSymbol typeSymbol)
            return;

        if (!SyntaxValidator.SyntaxIsValid(typeSyntax, productionContext))
            return;
    }
}