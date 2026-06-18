using AzotSerializer.Extensions;
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

        var collectionInterface = collection.AllInterfaces
            .FirstOrDefault(x => x.OriginalDefinition.ToDisplayString() == CollectionSupportedTypes.CollectionInterface);
        
        if (collectionInterface is null)
            return false;
        
        var elementType = collectionInterface.TypeArguments[0];
        var collectionTypeName = collection
            .WithNullableAnnotation(NullableAnnotation.None)
            .ToDisplayString();
        var casted = SyntaxBuilder.CastTo(collectionVar, collectionInterface.ToDisplayString());
        
        builder
            .Initialize("int", "count", "buffer.ReadInt32()")
            .Assign(collectionVar, $"new {collectionTypeName}(count)")
            .For("int i = 0", "i < count", "i++",
                body =>
                {
                    const string readVar = "collectionElement";
                    ReadWriteGenerator.GenerateReadForType(readVar, elementType, body);
                    body.Expression($"{casted}.Add({readVar})");
                });

        return true;
    }
}

public class ABC
{
    private Dictionary<int, string> _dictionary;

    public void A()
    {
        ((ICollection<KeyValuePair<int, string>>)_dictionary).Add(new KeyValuePair<int, string>(1, "ABC"));
    }
}