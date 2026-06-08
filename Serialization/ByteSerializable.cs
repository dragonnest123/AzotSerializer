using System.Buffers;
using Serialization.Extensions;

namespace Serialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ByteSerializableAttribute : Attribute 
{
    
}