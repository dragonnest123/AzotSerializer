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
                     

                     public static {{className}} Deserialize(ref ReadOnlySpan<byte> buffer)
                     {
                 {{EmitDeserializeBody(members, className)}}
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
        
        const string classVariableName = "deserializedClass";
        body.Declare(className, classVariableName, SyntaxBuilder.New(className));

        foreach (var member in members)
            EmitReadForMember(GetMemberType(member), $"{classVariableName}.{member.Name}", body);
        
        body.Return(classVariableName);

        return body.Build();
    }
    

    private static void EmitWriteForMember(string name, ITypeSymbol member, SyntaxBuilder builder)
    {
        string writeOp = string.Empty;
        
        var memberType = GetSpecialTypeString(member.SpecialType);
        if (memberType != null)
            writeOp = $"writer.Write{memberType}({name});";
        else if (member.TypeKind == TypeKind.Class || member.TypeKind == TypeKind.Struct)
            writeOp = $"{name}.Serialize(writer);";
        
        
        if (MemberCanBeNull(member))
            builder.If($"{name} is not null", body => body.AppendLines("writer.WriteByte(1);", writeOp))
                .Else(b => b.AppendLine("writer.WriteByte(0);"));
        else
            builder.AppendLines("writer.WriteByte(1);", writeOp);
    }

    private static void EmitReadForMember(ITypeSymbol member, string readVariable, SyntaxBuilder builder)
    {
        var memberType = GetSpecialTypeString(member.SpecialType);
        string readOp = string.Empty;

        if (memberType != null)
            readOp = $"buffer.Read{memberType}();";
        else if (member.TypeKind == TypeKind.Class || member.TypeKind == TypeKind.Struct)
            readOp = $"{member.Name}.Deserialize(ref buffer);";
        
        builder.If("buffer.ReadByte() != 0", body => body.Assign(readVariable, readOp));
    }
    
    private static ITypeSymbol GetMemberType(ISymbol member)
    {
        return member switch
        {
            IPropertySymbol p => p.Type,
            IFieldSymbol f    => f.Type
        };
    }
    
    private static string? GetSpecialTypeString(SpecialType specialType)
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
            SpecialType.System_Array   => "Array",
            SpecialType.System_Enum    => "Enum",
            _                          => null
        };
    }

    private static bool MemberCanBeNull(ITypeSymbol member)
        => member.NullableAnnotation == NullableAnnotation.Annotated || member.IsReferenceType;
}