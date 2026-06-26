using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace AzotSerializer.Extensions;

internal static class CodeAnalysisExtensions
{
    extension(ITypeSymbol typeSymbol)
    {
        public bool IsNullable()
            => typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;

        public bool IsNullableValueType() 
            => typeSymbol.IsValueType && typeSymbol.IsNullable();

        public bool IsEnum() 
            => typeSymbol is INamedTypeSymbol namedType && namedType.EnumUnderlyingType != null;
        
        public bool IsCollection() 
            => typeSymbol.AllInterfaces.Any(x => x.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.ICollection<T>");

        public bool IsNullableEnumType() 
            => typeSymbol.GetNullableValueUnderlyingType().IsEnum();

        public bool CanBeNull()
            => typeSymbol.NullableAnnotation == NullableAnnotation.Annotated || typeSymbol.IsReferenceType;

        public ITypeSymbol GetNullableValueUnderlyingType()
        {
            if (typeSymbol is INamedTypeSymbol namedType && typeSymbol.IsNullableValueType())
                return namedType.TypeArguments[0];
        
            return typeSymbol;
        }
        
        public string? GetSpecialTypeString()
        {
            return typeSymbol.SpecialType switch
            {
                SpecialType.System_Boolean    => "Bool",
                SpecialType.System_Byte       => "Byte",
                SpecialType.System_SByte      => "SByte",
                SpecialType.System_Char       => "Char",
                SpecialType.System_Int16      => "Int16",
                SpecialType.System_UInt16     => "UInt16",
                SpecialType.System_Int32      => "Int32",
                SpecialType.System_UInt32     => "UInt32",
                SpecialType.System_Int64      => "Int64",
                SpecialType.System_UInt64     => "UInt64",
                SpecialType.System_Single     => "Single",
                SpecialType.System_Double     => "Double",
                SpecialType.System_Decimal    => "Decimal",
                SpecialType.System_String     => "String",
                SpecialType.System_Array      => "Array",
                SpecialType.System_Enum       => "Enum",
                _                             => null
            };
        }
    }

    extension(ISymbol symbol)
    {
        public ITypeSymbol? GetPropertyOrFieldType()
        {
            return symbol switch
            {
                IPropertySymbol p => p.Type,
                IFieldSymbol f    => f.Type,
                _ => null
            };
        }
    }
}