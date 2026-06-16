using AzotSerializer.Extensions;
using Microsoft.CodeAnalysis;

namespace AzotSerializer.Generators;

public static class ObjectReadWriteGenerator
{
    public static void GenerateWriteForMember(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        builder.Expression($"{memberVar}.Serialize(writer)");
    }

    public static void GenerateReadForMember(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        builder.Assign(memberVar, $"{memberType.Name}.Deserialize(ref buffer)");
    }
}