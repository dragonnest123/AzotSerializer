using Microsoft.CodeAnalysis;

namespace AzotSerializer.Generators;

internal static class WellKnownTypesReadWriteGenerator
{
    private delegate void ReadWriteFunc(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder);

    private static readonly Dictionary<string, (ReadWriteFunc ReadFunc, ReadWriteFunc WriteFunc)> _handlers =
        new Dictionary<string, (ReadWriteFunc, ReadWriteFunc)>
        {
            [WellKnownSupportedTypes.DateTime] = (GenerateReadDateTime, GenerateWriteDateTime),
            [WellKnownSupportedTypes.TimeSpan] = (GenerateReadTimeSpan, GenerateWriteTimeSpan),
            [WellKnownSupportedTypes.KeyValuePair] = (GenerateReadKeyValuePair, GenerateWriteKeyValuePair),
        };
    
    public static bool TryGenerateWriteForMember(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        var memberFullName = memberType.OriginalDefinition.ToDisplayString();

        if (!_handlers.TryGetValue(memberFullName, out var handler))
            return false;
        
        handler.WriteFunc(memberVar, memberType, builder);
        return true;
    }

    public static bool TryGenerateReadForMember(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        var memberFullName = memberType.OriginalDefinition.ToDisplayString();

        if (!_handlers.TryGetValue(memberFullName, out var handler))
            return false;
        
        handler.ReadFunc(memberVar, memberType, builder);
        return true;
    }

    private static void GenerateWriteDateTime(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        builder.Expression($"writer.WriteDateTime({memberVar})");
    }

    private static void GenerateReadDateTime(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        builder.Assign(memberVar, "buffer.ReadDateTime()");
    }

    private static void GenerateWriteTimeSpan(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        builder.Expression($"writer.WriteTimeSpan({memberVar})");
    }

    private static void GenerateReadTimeSpan(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        builder.Assign(memberVar, "buffer.ReadTimeSpan()");
    }

    private static void GenerateWriteKeyValuePair(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        if (memberType is not INamedTypeSymbol namedTypeSymbol)
            return;
        
        var keyType = namedTypeSymbol.TypeArguments[0];
        var valueType = namedTypeSymbol.TypeArguments[1];

        ReadWriteGenerator.GenerateWriteForMember($"{memberVar}.Key", keyType, builder);
        ReadWriteGenerator.GenerateWriteForMember($"{memberVar}.Value", valueType, builder);
    }

    private static void GenerateReadKeyValuePair(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        if (memberType is not INamedTypeSymbol namedTypeSymbol)
            return;
        
        var keyType = namedTypeSymbol.TypeArguments[0];
        var valueType = namedTypeSymbol.TypeArguments[1];
        const string keyVar = "keyInKeyValuePair";
        const string valueVar = "valueInKeyValuePair";
        
        builder.Initialize(keyType.ToDisplayString(), keyVar, "default");
        builder.Initialize(valueType.ToDisplayString(), valueVar, "default");

        ReadWriteGenerator.GenerateReadForMember(keyVar, keyType, builder);
        ReadWriteGenerator.GenerateReadForMember(valueVar, valueType, builder);

        var keyTypeName = keyType.ToDisplayString();
        var valueTypeName = valueType.ToDisplayString();
        builder.Assign(memberVar, SyntaxBuilder.New($"KeyValuePair<{keyTypeName}, {valueTypeName}>", keyVar, valueVar));
    }
}