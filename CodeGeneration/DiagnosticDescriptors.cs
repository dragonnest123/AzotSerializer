using Microsoft.CodeAnalysis;

namespace AzotSerializer;

internal static class DiagnosticDescriptors
{
    private const string Category = "Generation";
    
    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "AZOTGEN001",
        title: "Serializable object must be partial",
        messageFormat: "Serializable object '{0}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static readonly DiagnosticDescriptor NestedContainingTypesMustBePartial = new(
        id: "AZOTGEN002",
        title: "Nested serializable object's containing type(s) must be partial",
        messageFormat: "Serializable object '{0}' containing type(s) must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static readonly DiagnosticDescriptor NotSupportedType = new(
        id: "AZOTGEN003",
        title: "Unsupported type",
        messageFormat: "Type '{0}' is not supported for serialization. " +
                       "Use natively supported types or types marked with [ByteSerializable].",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}