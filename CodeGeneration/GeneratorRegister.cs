using AzotSerializer.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AzotSerializer;

[Generator(LanguageNames.CSharp)]
internal class GeneratorRegister : IIncrementalGenerator
{
    public const string MetadataAttributeName = "Serialization.ByteSerializableAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var typeDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            MetadataAttributeName,
            predicate: static (node, _) => IsSyntaxTarget(node),
            transform: static (context, _) => (TypeDeclarationSyntax)context.TargetNode);
        
        var source = typeDeclarations.Combine(context.CompilationProvider);
        
        context.RegisterSourceOutput(source, static (context, source) =>
        {
            TypeSerializationGenerator.GenerateSerializer(source.Left, source.Right, context);
        });
    }
    
    private static bool IsSyntaxTarget(SyntaxNode node)
        => node is ClassDeclarationSyntax
            or StructDeclarationSyntax
            or RecordDeclarationSyntax;
}   

