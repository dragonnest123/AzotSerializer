namespace AzotSerializer.Utils;

public readonly struct TypeName : IEquatable<TypeName>
{
    public readonly string ContainingNamespace;
    public readonly string Name;

    public TypeName(string containingNamespace, string name)
    {
        ContainingNamespace = containingNamespace;
        Name = name;
    }

    public bool Equals(TypeName other)
        => ContainingNamespace == other.ContainingNamespace &&
           Name == other.Name;

    public override bool Equals(object? obj)
        => obj is TypeName other && Equals(other);

    public override int GetHashCode()
        => (ContainingNamespace, Name).GetHashCode();
}   