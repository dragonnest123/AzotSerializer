namespace Serialization.RuntimeSerialization;

internal class ObjectData
{
    public MemberAccessor[]? Accessors { get; set; }
    public Type? CollectionInterfaceType { get; set; }
}