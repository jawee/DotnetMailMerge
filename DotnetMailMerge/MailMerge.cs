namespace DotnetMailMerge;

public class MailMerge
{
    private string _template;
    private readonly Dictionary<string, object> _parameters;
    private readonly Parser _parser;
    public MailMerge(string template, Dictionary<string, object> parameters)
    {
        _template = template;
        _parameters = parameters;
        var lexer = new Lexer(template);
        _parser = new Parser(lexer);
    }

    public Result<string, Exception> Render() 
    {
        var ast = _parser.Parse();

        var res = "";
        foreach (var block in ast.Blocks)
        {
            res += block switch
            {
                IfBlock => HandleIfBlock(block),
                TextBlock => HandleTextBlock(block),
                ReplaceBlock => HandleReplaceBlock(block),
                _ => throw new NotImplementedException("unknown block")
            };
            
        }

        return res;
    }

    private string HandleReplaceBlock(Block block)
    {
        var b = block as ReplaceBlock;
        var res = _parameters[b.Property];

        return res.ToString();
    }

    private string HandleTextBlock(Block block)
    {
        var b = block as TextBlock;

        return b.Text;
    }

    private string HandleIfBlock(Block block)
    {
        throw new NotImplementedException();
    }
}

