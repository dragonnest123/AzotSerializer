namespace Serialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ByteSerializableAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ByteIgnoreAttribute : Attribute;