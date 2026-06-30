using System.Text;
using AzotSerializer.Extensions;

namespace AzotSerializer;

internal class SyntaxBuilder
{
    private const string Tabulation = "    ";
        
    private readonly StringBuilder _builder = new StringBuilder();
    private string _indent;
    private int _varCount;
    
    public SyntaxBuilder(string initialIndent = "")
    {
        _indent = initialIndent;
    }
    
    public string Build() => _builder.ToString();
    
    public static string New(string className, params string[] args)
        => $"new {className}({string.Join(", ", args)})";
    
    public static string CastTo(string variable, string typeToCast)
        => $"(({typeToCast}){variable})";

    public string NextVar(string varPrefix)
        => $"{varPrefix}{_varCount++}";
    
    public SyntaxBuilder AppendLine(string line)
    {
        _builder.AppendLineWithIndent(_indent, line);

        return this;
    }
    

    public SyntaxBuilder Expression(string line)
    {
        _builder.AppendLineWithIndent(_indent, $"{line};");
        
        return this;
    }
    
    public SyntaxBuilder Expressions(params string[] lines)
    {
        foreach (var line in lines)
            _builder.AppendLineWithIndent(_indent, $"{line};");
        
        return this;
    }

    public SyntaxBuilder Assign(string variable, string value)
    {
        return Expression($"{variable} = {value}");
    }

    public SyntaxBuilder Initialize(string type, string varName, string value)
    {
        return Expression($"{type} {varName} = {value}");
    }

    public SyntaxBuilder Return(string returnValue)
    {
        return Expression($"return {returnValue}");
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
    
    public SyntaxBuilder For(string init, string condition, string action, Action<SyntaxBuilder> body)
    {
        _builder.AppendLineWithIndent(_indent, $"for ({init}; {condition}; {action})");
        
        return BuildBody(body);
    }

    public SyntaxBuilder Foreach(string collectionName, Action<SyntaxBuilder> body)
    {
        _builder.AppendLineWithIndent(_indent, $"foreach (var item in {collectionName})");
        
        return BuildBody(body);
    }

    public SyntaxBuilder While(string condition, Action<SyntaxBuilder> body)
    {
        _builder.AppendLineWithIndent(_indent, $"while ({condition})");
        
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