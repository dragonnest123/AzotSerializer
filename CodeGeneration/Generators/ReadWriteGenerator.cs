using AzotSerializer.Extensions;
using Microsoft.CodeAnalysis;

namespace AzotSerializer.Generators;

internal static class ReadWriteGenerator
{
    public static void GenerateWriteForMember(string memberVar, ITypeSymbol member, SyntaxBuilder builder)
    {
        if (member.CanBeNull())
            builder.If($"{memberVar} is not null", body =>
                {
                    body.Expression("writer.WriteByte(1)");
                    GenerateWrite(memberVar, member, body);
                })
                .Else(b => b.Expression("writer.WriteByte(0)"));
        else
            GenerateWrite(memberVar, member, builder);
    }

    public static void GenerateReadForMember(string memberVar, ITypeSymbol member, SyntaxBuilder builder)
    {
        if (member.CanBeNull())
            builder.If("buffer.ReadByte() != 0", body => GenerateRead(memberVar, member, body));
        else
            GenerateRead(memberVar, member, builder);
    }
    
    public static void GenerateReadForType(string declaredVar, ITypeSymbol member, SyntaxBuilder builder)
    {
        var type = member.ToDisplayString();
        
        builder.Initialize(type, declaredVar, "default");
        if (member.CanBeNull())
            builder.If("buffer.ReadByte() != 0", body => GenerateRead(declaredVar, member, body));
        else
            GenerateRead(declaredVar, member, builder);
    }
    
    private static void GenerateWrite(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        memberVar = memberType.IsNullableValueType() ? $"{memberVar}.Value" : memberVar;
        var notNullable = memberType.GetNullableValueUnderlyingType();

        if (PrimitiveTypeReadWriteGenerator.TryGenerateWriteForMember(memberVar, notNullable, builder) ||
            WellKnownTypesReadWriteGenerator.TryGenerateWriteForMember(memberVar, notNullable, builder) ||
            CollectionReadWriteGenerator.TryGenerateWriteForMember(memberVar, notNullable, builder))
        {
            return;
        }
        
        ObjectReadWriteGenerator.GenerateWriteForMember(memberVar, notNullable, builder);
    }

    private static void GenerateRead(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        var notNullable = memberType.GetNullableValueUnderlyingType();

        if (PrimitiveTypeReadWriteGenerator.TryGenerateReadForMember(memberVar, notNullable, builder) ||
            WellKnownTypesReadWriteGenerator.TryGenerateReadForMember(memberVar, notNullable, builder) ||
            CollectionReadWriteGenerator.TryGenerateReadForMember(memberVar, notNullable, builder))
        {
            return;
        }
        
        ObjectReadWriteGenerator.GenerateReadForMember(memberVar, notNullable, builder);
    }
}