using AzotSerializer.Extensions;
using Microsoft.CodeAnalysis;

namespace AzotSerializer.Generators;

internal static class PrimitiveTypeReadWriteGenerator
{
    public static bool TryGenerateWriteForMember(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        if (memberType.IsEnum())
        {
            memberType = ((INamedTypeSymbol)memberType).EnumUnderlyingType!;
            memberVar = SyntaxBuilder.CastTo(memberVar, memberType.ToDisplayString());
        }
        
        if (!TryGetPrimitiveType(memberType, out var type))
            return false;
        
        builder.Expression($"writer.Write{type}({memberVar})");
        return true;
    }

    public static bool TryGenerateReadForMember(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        string? stringType;
        if (memberType.IsEnum())
        {
            var underlying = ((INamedTypeSymbol)memberType).EnumUnderlyingType!;
            TryGetPrimitiveType(underlying, out stringType);
            
            var castedValue = SyntaxBuilder.CastTo($"buffer.Read{stringType}()", memberType.ToDisplayString());
            builder.Assign(memberVar, castedValue);
            
            return true;
        }
        
        if (!TryGetPrimitiveType(memberType, out stringType))
            return false;
        
        builder.Assign(memberVar, $"buffer.Read{stringType}()");
        return true;
    }

    private static bool TryGetPrimitiveType(ITypeSymbol memberType, out string? type)
    {
        type = memberType.GetSpecialTypeString();
        
        return type is not null;
    }
}