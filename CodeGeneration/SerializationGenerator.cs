using AzotSerializer.Extensions;
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
        var typeName = typeSymbol.Name;
        var typeKind = GetMemberKindString(typeSymbol);
        var members = typeSymbol
            .GetMembers()
            .Where(symbol => (symbol.Kind == SymbolKind.Field || symbol.Kind == SymbolKind.Property) 
                    && !symbol.IsImplicitlyDeclared)
            .ToArray();
        
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

                 namespace {{@namespace}};

                 public partial {{typeKind}} {{typeName}}
                 {
                     {{constructor}}
                 
                     public void Serialize(IBufferWriter<byte> writer)
                     {
                 {{EmitSerializeBody(members)}}
                     }
                     
                     public ReadOnlySpan<byte> Serialize()
                     {
                         var writer = new ArrayBufferWriter<byte>();
                         Serialize(writer);
                         
                         return writer.WrittenSpan;
                     }
                     

                     public static {{typeName}} Deserialize(ref ReadOnlySpan<byte> buffer)
                     {
                 {{EmitDeserializeBody(members, typeName)}}
                     }
                 }
                 """;
    }
    
    private static string EmitSerializeBody(ISymbol[] members)
    {
        var body = new SyntaxBuilder(new string(' ', 8));
        
        foreach (var member in members)
            EmitWriteForMember(member.Name, GetMemberType(member), body);
        
        return body.Build();
    } 

    private static string EmitDeserializeBody(ISymbol[] members, string className)
    {
        var body = new SyntaxBuilder(new string(' ', 8));
        
        const string classVariableName = "deserializedObject";
        body.Declare(className, classVariableName, SyntaxBuilder.New(className));

        foreach (var member in members)
            EmitReadForMember(GetMemberType(member), $"{classVariableName}.{member.Name}", body);
        
        body.Return(classVariableName);

        return body.Build();
    }

    private static void EmitWriteForMember(string name, ITypeSymbol member, SyntaxBuilder builder)
    {
        string writeOp;
        var nullableValueName = member.IsNullableValueType() ? $"{name}.Value" : name;
        string? specialType = member
            .GetNullableValueUnderlyingType()
            .GetSpecialTypeString();

        if (specialType != null)
            writeOp = $"writer.Write{specialType}({nullableValueName});";
        else
            writeOp = $"{nullableValueName}.Serialize(writer);";
        
        if (member.CanBeNull())
            builder.If($"{name} is not null", body => body.AppendLines("writer.WriteByte(1);", writeOp))
                .Else(b => b.AppendLine("writer.WriteByte(0);"));
        else
            builder.AppendLine(writeOp);
    }

    private static void EmitReadForMember(ITypeSymbol member, string readVariable, SyntaxBuilder builder)
    {
        var notNullable = member.GetNullableValueUnderlyingType();
        var typeName = notNullable.GetSpecialTypeString();
        string readOp = string.Empty;

        if (typeName != null)
            readOp = $"buffer.Read{typeName}();";
        else if (member.TypeKind == TypeKind.Class || member.TypeKind == TypeKind.Struct)
            readOp = $"{notNullable.Name}.Deserialize(ref buffer);";

        if (member.CanBeNull())
            builder.If("buffer.ReadByte() != 0", body => body.Assign(readVariable, readOp));
        else
            builder.Assign(readVariable, readOp);
    }
    
    private static ITypeSymbol GetMemberType(ISymbol member)
    {
        return member switch
        {
            IPropertySymbol p => p.Type,
            IFieldSymbol f    => f.Type,
        };
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