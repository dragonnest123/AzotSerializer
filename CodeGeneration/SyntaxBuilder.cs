using System.Text;
using AzotSerializer.Extensions;

namespace AzotSerializer;

public class SyntaxBuilder
{
    private const string Tabulation = "    ";
        
    private readonly StringBuilder _builder = new StringBuilder();
    private string _indent;
    
    public SyntaxBuilder(string initialIndent = "")
    {
        _indent = initialIndent;
    }
    
    public string Build() => _builder.ToString();
    
    public static string New(string className, params string[] args)
        => $"new {className}({string.Join(", ", args)})";

    public static string Call(string objectName, string methodName, params string[] args)
        => $"{objectName}.{methodName}({string.Join(", ", args)})";

    public static string Call(string methodName, params string[] args)
        => $"{methodName}({string.Join(", ", args)})";
    
    
    public SyntaxBuilder AppendLine(string line)
    {
        _builder.AppendLineWithIndent(_indent, line);
        
        return this;
    }

    public SyntaxBuilder AppendLines(params string[] lines)
    {
        foreach (var line in lines)
            _builder.AppendLineWithIndent(_indent, line);

        return this;
    }

    public SyntaxBuilder Assign(string variable, string value)
    {
        _builder.AppendLineWithIndent(_indent, $"{variable} = {value}");
        
        return this;
    }
    
    public SyntaxBuilder Declare(string type, string varName, string value)
    {
        _builder.AppendLineWithIndent(_indent, $"{type} {varName} = {value};");
        
        return this;
    }

    public SyntaxBuilder MethodCall(string methodName, string? objectName, params string[] methodParams)
    {
        if (objectName == null)
            _builder.AppendLineWithIndent(_indent, $"{methodName}({string.Join(", ", methodParams)})");
        else
            _builder.AppendLineWithIndent(_indent, $"{objectName}.{methodName}({string.Join(", ", methodParams)})");

        return this;
    }

    public SyntaxBuilder Return(string returnValue)
    {
        _builder.AppendWithIndent(_indent, $"return {returnValue};");

        return this;
    }
    
    public SyntaxBuilder If(string condition, Action<SyntaxBuilder> body)
    {
        _builder.AppendLineWithIndent(_indent, $"if ({condition})");

        return BuildBody(body);
    }
    
    public SyntaxBuilder Elseif(string condition, Action<SyntaxBuilder> body)
    {
        _builder.AppendLineWithIndent(_indent, $"else if ({condition})");
        
        return BuildBody(body);
    }
    
    public SyntaxBuilder Else(Action<SyntaxBuilder> body)
    {
        _builder.AppendLineWithIndent(_indent, "else");

        return BuildBody(body);
    }
    
    public SyntaxBuilder OpenBlock()
    {
        _builder.AppendLineWithIndent(_indent, "{");
        _indent += Tabulation;
        return this;
    }

    public SyntaxBuilder CloseBlock()
    {
        _indent = _indent.Substring(Tabulation.Length);
        _builder.AppendLineWithIndent(_indent, "}");
        return this;
    }

    private SyntaxBuilder BuildBody(Action<SyntaxBuilder> body)
    {
        OpenBlock();
        body(this);
        CloseBlock();

        return this;
    }
}