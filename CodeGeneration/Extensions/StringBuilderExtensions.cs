using System.Text;

namespace AzotSerializer.Extensions;

internal static class StringBuilderExtensions
{
    extension(StringBuilder sb)
    {
        public void AppendWithIndent(string indent, string value)
        {
            sb.Append(indent);
            sb.Append(value);
        }

        public void AppendLineWithIndent(string indent, string value)
        {
            sb.Append(indent);
            sb.AppendLine(value);
        }
    }
}