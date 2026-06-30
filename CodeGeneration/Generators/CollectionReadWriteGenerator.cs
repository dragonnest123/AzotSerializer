using Microsoft.CodeAnalysis;

namespace AzotSerializer.Generators;

internal static class CollectionReadWriteGenerator
{
    public static bool TryGenerateWriteForMember(string collectionVar, ITypeSymbol collection, SyntaxBuilder builder)
    {
        var collectionInterface = collection.AllInterfaces
            .FirstOrDefault(x => x.OriginalDefinition.ToDisplayString() == CollectionSupportedTypes.CollectionInterface);
        
        if (collectionInterface is null)
            return false;
        
        var elementType = collectionInterface.TypeArguments[0];
        var enumeratorVar = builder.NextVar("enumerator");
        
        builder
            .Expression($"writer.WriteInt32({collectionVar}.Count)")
            .Initialize("var", enumeratorVar, $"{collectionVar}.GetEnumerator()")
            .While($"{enumeratorVar}.MoveNext()", 
                x => ReadWriteGenerator.GenerateWriteForMember($"{enumeratorVar}.Current", elementType, x));

        return true;
    }

    public static bool TryGenerateReadForMember(string collectionVar, ITypeSymbol collection, SyntaxBuilder builder)
    {
        var collectionInterface = collection.AllInterfaces
            .FirstOrDefault(x => x.OriginalDefinition.ToDisplayString() == CollectionSupportedTypes.CollectionInterface);
        
        if (collectionInterface is null)
            return false;
        
        var elementType = collectionInterface.TypeArguments[0];
        var collectionTypeName = collection
            .WithNullableAnnotation(NullableAnnotation.None)
            .ToDisplayString();
        var casted = SyntaxBuilder.CastTo(collectionVar, collectionInterface.ToDisplayString());
        
        var countVar = builder.NextVar("count");
        var cycleVar = builder.NextVar("i");
        
        builder
            .Initialize("int", countVar, "buffer.ReadInt32()")
            .Assign(collectionVar, $"new {collectionTypeName}({countVar})")
            .For($"int {cycleVar} = 0", $"{cycleVar} < {countVar}", $"{cycleVar}++",
                body =>
                {
                    string readVar = builder.NextVar("collectionElement");
                    ReadWriteGenerator.GenerateReadForType(readVar, elementType, body);
                    body.Expression($"{casted}.Add({readVar})");
                });

        return true;
    }
}