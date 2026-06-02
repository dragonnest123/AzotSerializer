using System.Text;
using Microsoft.CodeAnalysis;
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

        if (semanticModel.GetDeclaredSymbol(typeSyntax) is not INamedTypeSymbol typeSymbol)
            return;

        if (!SyntaxValidator.SyntaxIsValid(typeSyntax, productionContext))
            return;

        productionContext.AddSource($"{typeSymbol.Name}Serializer.g.cs", EmitSerialization(typeSymbol));
    }
    
    private static string EmitSerialization(INamedTypeSymbol typeSymbol)
    {
        var @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
        var className = typeSymbol.Name;
        var members = typeSymbol
            .GetMembers()
            .Where(symbol => (symbol.Kind == SymbolKind.Field && !symbol.IsImplicitlyDeclared) 
                             || symbol.Kind == SymbolKind.Property)
            .ToArray();
        
        var hasParameterlessConstructor = typeSymbol.Constructors
            .Any(c => c.Parameters.Length == 0 && !c.IsImplicitlyDeclared);
        
        var constructor = hasParameterlessConstructor 
            ? "" 
            : $"public {className}() {{ }}";
        
        return $$"""
                 using System;
                 using System.Buffers;
                 using System.Buffers.Binary;
                 using System.Text;
                 using Serialization.Extensions;

                 namespace {{@namespace}};

                 public partial class {{className}}
                 {
                     {{constructor}}
                 
                     public static void Serialize({{className}} value, IBufferWriter<byte> writer)
                     {
                         {{EmitSerializeBody(members)}}
                     }
                     
                     public static ReadOnlySpan<byte> Serialize({{className}} value)
                     {
                         var writer = new ArrayBufferWriter<byte>();
                         Serialize(value, writer);
                         
                         return writer.WrittenSpan;
                     }
                     

                     public static {{className}} Deserialize(ref ReadOnlySpan<byte> buffer)
                     {
                         {{EmitDeserializeBody(members, className)}}
                     }
                 }
                 """;
    }
    
    private  static string EmitSerializeBody(ISymbol[] members)
    {
        var body = new StringBuilder();
        var indent = new string(' ', 8);
        
        foreach (var member in members)
            body.AppendLine(indent + EmitWriteForMember(member.Name, GetMemberType(member)));
        
        return body.ToString();
    }

    private static string EmitDeserializeBody(ISymbol[] members, string className)
    {
        var body = new StringBuilder();
        var indent = new string(' ', 8);
        
        const string classVariableName = "deserializedClass";
        var classCreation = $"var {classVariableName} = new {className}();";
        body.AppendLine(classCreation);

        foreach (var member in members)
        {
            var assignMemberValue = $"{classVariableName}.{member.Name} = {EmitReadForMember(GetMemberType(member))};";
            body.AppendLine(indent + assignMemberValue);
        }
        
        body.AppendLine(indent + $"return {classVariableName};");
        
        return body.ToString();
    }

    private static string EmitWriteForMember(string name, ITypeSymbol member)
    {
        var memberType = GetTypeString(member.SpecialType) ?? "";
        
        return $"writer.Write{memberType}(value.{name});";
    }

    private static string EmitReadForMember(ITypeSymbol member)
    {
        var memberType = GetTypeString(member.SpecialType) ?? "";
        
        return $"buffer.Read{memberType}();";
    }
    
    private static ITypeSymbol GetMemberType(ISymbol member)
    {
        return member switch
        {
            IPropertySymbol p => p.Type,
            IFieldSymbol f    => f.Type
        };
    }
    
    private static string? GetTypeString(SpecialType specialType)
    {
        return specialType switch
        {
            SpecialType.System_Boolean => "Bool",
            SpecialType.System_Byte    => "Byte",
            SpecialType.System_SByte   => "SByte",
            SpecialType.System_Int16   => "Int16",
            SpecialType.System_UInt16  => "UInt16",
            SpecialType.System_Int32   => "Int32",
            SpecialType.System_UInt32  => "UInt32",
            SpecialType.System_Int64   => "Int64",
            SpecialType.System_UInt64  => "UInt64",
            SpecialType.System_Single  => "Single",
            SpecialType.System_Double  => "Double",
            SpecialType.System_Decimal => "Decimal",
            SpecialType.System_String  => "String",
            _                          => null
        };
    }
}