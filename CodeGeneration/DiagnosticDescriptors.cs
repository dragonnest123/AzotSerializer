using Microsoft.CodeAnalysis;

namespace AzotSerializer;

public class DiagnosticDescriptors
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
}