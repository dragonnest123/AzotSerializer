using AzotSerializer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AzotSerializer.Generators;

internal static class TypeSerializationGenerator
{
    public static void GenerateSerializer(
        TypeDeclarationSyntax typeSyntax, 
        Compilation compilation,
        SourceProductionContext productionContext)
    {
        var semanticModel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);

        if (semanticModel.GetDeclaredSymbol(typeSyntax) is not INamedTypeSymbol typeSymbol)
            return;
        
        var typeMembers = SerializationDataProvider.GetTargetSerializationMembers(typeSymbol);
        if (!SyntaxValidator.IsTypeSerializable(typeSyntax, typeMembers, productionContext))
            return;

        productionContext.AddSource($"{typeSymbol.Name}Serializer.g.cs", EmitSerializationFile(typeSymbol, typeMembers));
    }
    
    private static string EmitSerializationFile(
        INamedTypeSymbol typeSymbol,
        ISymbol[] typeMembers)
    {
        var @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
        var typeName = typeSymbol.Name;
        var typeKind = GetMemberKindString(typeSymbol);
        
        var hasParameterlessConstructor = typeSymbol.Constructors
            .Any(c => c.Parameters.Length == 0 && !c.IsImplicitlyDeclared);
        
        var constructor = hasParameterlessConstructor 
            ? "" 
            : $"public {typeName}() {{ }}";
        
        return $$"""
                 using System;
                 using System.Buffers;
                 using System.Buffers.Binary;
                 using System.Text;
                 using Serialization.Extensions;
                 using Serialization.RuntimeSerialization.Serializers;

                 namespace {{@namespace}};

                 public partial {{typeKind}} {{typeName}}
                 {
                     {{constructor}}
                 
                     public void Serialize(ArrayBufferWriter<byte> writer)
                     {
                 {{EmitSerializeBody(typeMembers)}}
                     }
                     
                     public ReadOnlySpan<byte> Serialize()
                     {
                         var writer = new ArrayBufferWriter<byte>();
                         Serialize(writer);
                         
                         return writer.WrittenSpan;
                     }
                     

                     public static {{typeName}} Deserialize(ref ReadOnlySpan<byte> buffer)
                     {
                 {{EmitDeserializeBody(typeMembers, typeName)}}
                     }
                 }
                 """;
    }
    
    private static string EmitSerializeBody(ISymbol[] members)
    {
        var body = new SyntaxBuilder(new string(' ', 8));

        foreach (var member in members)
        {
            var typeSymbol = member.GetPropertyOrFieldType();
            if (typeSymbol is null)
                return body.AppendLine($"{member.ToDisplayString()} is not a property or field").Build();
            
            ReadWriteGenerator.GenerateWriteForMember(member.Name, typeSymbol, body);
        }
        
        return body.Build();
    }

    private static string EmitDeserializeBody(ISymbol[] members, string className)
    {
        var body = new SyntaxBuilder(new string(' ', 8));
        
        const string classVariableName = "deserializedObject";
        body.Initialize(className, classVariableName, SyntaxBuilder.New(className));

        foreach (var member in members)
        {
            var typeSymbol = member.GetPropertyOrFieldType();
            if (typeSymbol is null)
                return body.AppendLine($"{member.ToDisplayString()} is not a property or field").Build();
            
            ReadWriteGenerator.GenerateReadForMember($"{classVariableName}.{member.Name}", typeSymbol, body);
        }
        
        body.Return(classVariableName);

        return body.Build();
    }
    
    private static string? GetMemberKindString(INamedTypeSymbol namedTypeSymbol)
    {
        return (namedTypeSymbol.TypeKind, namedTypeSymbol.IsRecord) switch
        {
            (TypeKind.Class, false)  => "class",
            (TypeKind.Class, true)   => "record",
            (TypeKind.Struct, false) => "struct",
            (TypeKind.Struct, true)  => "record struct",
            _ => null
        };
    }
}