using AzotSerializer.Extensions;
using Microsoft.CodeAnalysis;

namespace AzotSerializer.Generators;

internal static class PrimitiveTypeReadWriteGenerator
{
    public static bool TryGenerateWriteForMember(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        if (!TryGetPrimitiveType(memberType, out var type))
            return false;
        
        builder.Expression($"writer.Write{type}({memberVar})");
        return true;
    }

    public static bool TryGenerateReadForMember(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        if (!TryGetPrimitiveType(memberType, out var type))
            return false;
        
        builder.Assign(memberVar, $"buffer.Read{type}()");
        return true;
    }

    private static bool TryGetPrimitiveType(ITypeSymbol memberType, out string? type)
    {
        type = memberType.GetSpecialTypeString();
        
        return type is not null;
    }
}