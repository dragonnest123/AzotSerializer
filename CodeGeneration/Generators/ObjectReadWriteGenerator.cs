using Microsoft.CodeAnalysis;

namespace AzotSerializer.Generators;

internal static class ObjectReadWriteGenerator
{
    public static void GenerateWriteForMember(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        if (memberType.SpecialType != SpecialType.System_Object)
        {
            builder.Expression($"{memberVar}.Serialize(writer)");
            return;
        }
        
        const string serializeAction = "serializeAction";
        const string builderCall = "ObjectSerializer.BuildSerializer<object>()";

        builder
            .Initialize("var", serializeAction, builderCall)
            .Expression($"{serializeAction}({memberVar}, writer)");
    }

    public static void GenerateReadForMember(string memberVar, ITypeSymbol memberType, SyntaxBuilder builder)
    {
        if (memberType.SpecialType != SpecialType.System_Object)
        {
            builder.Assign(memberVar, $"{memberType.Name}.Deserialize(ref buffer)");
            return;
        }
        
        const string deserializeAction = "deserializeAction";
        const string builderCall = "ObjectSerializer.BuildDeserializer<object>()";
        
        builder
            .Initialize("var", deserializeAction, builderCall)
            .Assign(memberVar, $"{builderCall}(ref buffer)");
    }
}