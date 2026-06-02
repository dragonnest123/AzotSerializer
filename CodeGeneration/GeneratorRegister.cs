using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AzotSerializer;

[Generator(LanguageNames.CSharp)]
internal class GeneratorRegister : IIncrementalGenerator
{
    private const string MetadataAttributeName = "Serialization.ByteSerializableAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var typeDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            MetadataAttributeName,
            predicate: static (node, ct) => IsSyntaxTarget(node),
            transform: static (context, ct) => (TypeDeclarationSyntax)context.TargetNode);
        
        var source = typeDeclarations.Combine(context.CompilationProvider);
        
        context.RegisterSourceOutput(source, static (context, source) =>
        {
            SerializationGenerator.Generate(source.Left, source.Right, context);
        });
    }
    
    private static bool IsSyntaxTarget(SyntaxNode node)
        => node is ClassDeclarationSyntax
            or StructDeclarationSyntax
            or RecordDeclarationSyntax;
}   

