using System.Collections;
using AzotSerializer.Extensions;
using Microsoft.CodeAnalysis;

namespace AzotSerializer.Generators;

internal static class CollectionReadWriteGenerator
{
    public static bool TryGenerateWriteForMember(string collectionVar, ITypeSymbol collection, SyntaxBuilder builder)
    {
        var collectionInterface = collection.AllInterfaces
            .FirstOrDefault(x => x.OriginalDefinition.ToDisplayString() == CollectionSupportedTypes.GenericCollectionInterface);
        
        if (collectionInterface is null)
            return false;
        
        var elementType = collectionInterface.TypeArguments[0];
        var enumeratorVar = "enumerator";
        
        builder
            .Expression($"writer.WriteInt32({collectionVar}.Count)")
            .Initialize("var", enumeratorVar, $"{collectionVar}.GetEnumerator()")
            .While($"{enumeratorVar}.MoveNext()", 
                x => ReadWriteGenerator.GenerateWriteForMember($"{enumeratorVar}.Current", elementType, x));

        return true;
    }

    public static bool TryGenerateReadForMember(string collectionVar, ITypeSymbol collection, SyntaxBuilder builder)
    {
        if (collection is not INamedTypeSymbol namedType)
        {
            builder.AppendLine("member is not INamedTypeSymbol");
            return false;
        }
        
        if (!collection.IsCollection())
            return false;
        
        var elementType = namedType.TypeArguments[0];
        var collectionTypeName = collection
            .WithNullableAnnotation(NullableAnnotation.None)
            .ToDisplayString();

        builder
            .Initialize("int", "count", "buffer.ReadInt32()")
            .Assign(collectionVar, $"new {collectionTypeName}(count)")
            .For("int i = 0", "i < count", "i++", 
                body =>
                {
                    const string readVar = "collectionElement";
                    ReadWriteGenerator.GenerateReadForType(readVar, elementType, body);
                    body.Expression($"{collectionVar}.Add({readVar})");
                });

        return true;
    }
}