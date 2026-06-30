using Microsoft.CodeAnalysis;

namespace AzotSerializer.Generators;

internal static class ArrayReadWriteGenerator
{
    public static bool TryGenerateWriteForMember(string arrayVar, ITypeSymbol array, SyntaxBuilder builder)
    {
        if (array is not IArrayTypeSymbol arrayType)
            return false;

        var elementType = arrayType.ElementType;
        
        var cycleVar = builder.NextVar("i");
        builder
            .Expression($"writer.WriteInt32({arrayVar}.Length)")
            .For($"int {cycleVar} = 0", $"{cycleVar} < {arrayVar}.Length", $"{cycleVar}++",
                body => ReadWriteGenerator.GenerateWriteForMember($"{arrayVar}[{cycleVar}]", elementType, body));

        return true;
    }

    public static bool TryGenerateReadForMember(string arrayVar, ITypeSymbol array, SyntaxBuilder builder)
    {
        if (array is not IArrayTypeSymbol arrayType)
            return false;

        var elementType = arrayType.ElementType;
        var elementTypeName = elementType
            .WithNullableAnnotation(NullableAnnotation.None)
            .ToDisplayString();
        
        string lengthVar = builder.NextVar("length");
        var cycleVar = builder.NextVar("i");
        builder
            .Initialize("int", lengthVar, "buffer.ReadInt32()")
            .Assign(arrayVar, $"new {BuildArrayDeclaration(elementTypeName, lengthVar)}")
            .For($"int {cycleVar} = 0", $"{cycleVar} < {lengthVar}", $"{cycleVar}++",
                body =>
                {
                    string readVar = builder.NextVar("arrayElement");
                    ReadWriteGenerator.GenerateReadForType(readVar, elementType, body);
                    body.Expression($"{arrayVar}[{cycleVar}] = {readVar}");
                });

        return true;
    }
    
    private static string BuildArrayDeclaration(string elementTypeName, string lengthVar)
    {
        var bracketIndex = elementTypeName.IndexOf('[');
        if (bracketIndex == -1)
            return $"{elementTypeName}[{lengthVar}]";      
                               
        return elementTypeName.Substring(0, bracketIndex) 
               + $"[{lengthVar}]" 
               + elementTypeName.Substring(bracketIndex); 
    }
}