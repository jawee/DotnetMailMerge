namespace DotnetMailMerge;

public class Parser
{
    private readonly Lexer _lexer;
    private Token _curToken;
    private Token _peekToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;

        NextToken();
        NextToken();
    }

    private void NextToken()
    {
        _curToken = _peekToken;
        _peekToken = _lexer.GetNextToken();
    }

    public Ast Parse()
    {
        var blocks = new List<Block>();

        while (_curToken.TokenType != TokenType.EOF)
        {
            var block = ParseBlock();
            if (block is not null)
            {
                blocks.Add(block);
            }
            //NextToken();
        }

        return new() { Blocks = blocks };
    }

    private Block? ParseBlock() 
    {
        return _curToken.TokenType switch
        {
            TokenType.Start => ParseLogicBlock(),
            TokenType.Character => ParseTextBlock(),
            _ => throw new Exception($"ParseBlock _ matched. {_curToken.TokenType} {_curToken.Literal}"),
        } ;
    }

    private TextBlock? ParseTextBlock()
    {
        var res = "";
        while (_curToken.TokenType is TokenType.Character)
        {
            res += _curToken.Literal;
            NextToken();
        }
        return new() { Text = res };
    }

    private List<Block> ParseConsequence()
    {
        NextToken();
        var blocks = new List<Block>();

        while (!(_curToken.TokenType == TokenType.Start && _peekToken.Literal == "/"))
        {
            var block = ParseBlock();
            if (block is not null)
            {
                blocks.Add(block);
            }
            //NextToken();
        }

        return blocks;
    }

    private Block? ParseIf()
    {
        //if asdf }} consequence {{/if}}
        NextToken(); //i
        NextToken(); //f
        NextToken(); //space
        NextToken();
        var condition = "";
        while (_curToken.TokenType != TokenType.End)
        {
            condition += _curToken.Literal;
            NextToken();
        }

        var consequence = ParseConsequence();

        if (_curToken.TokenType != TokenType.Start)
        {
            throw new Exception($"Not sure this should happen. {_curToken.TokenType} {_curToken.Literal}");
        }

        NextToken(); // slash
        NextToken(); // i
        NextToken(); // f
        NextToken(); // end
        NextToken();

        return new IfBlock { Condition = condition.Trim(), Consequence = consequence };
    }

    private ReplaceBlock? ParseReplacement()
    {
        var res = "";

        while (_curToken.TokenType != TokenType.End)
        {
            res += _curToken.Literal;
            NextToken();
        }

        NextToken();

        return new() { Property = res.Trim() };
    }

    private Block? ParseLogicBlock()
    {
        NextToken();

        var block = _curToken.Literal switch
        {
            "#" => ParseIf(),
            _ => ParseReplacement()
        };

        return block;
    }
}

public class Ast 
{
    public List<Block> Blocks { get; set; } = default!;
}

public interface Block
{ 
}

public class TextBlock : Block
{
    public string Text { get; set; } = default!;

    public override bool Equals(object? obj)
    {
        return obj is TextBlock block &&
               Text == block.Text;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Text);
    }
}

public class IfBlock : Block 
{
    public string Condition { get; set; } = default!;
    public List<Block> Consequence { get; set; } = default!;

    public override bool Equals(object? obj)
    {
        return obj is IfBlock block &&
               Condition == block.Condition &&
               EqualityComparer<List<Block>>.Default.Equals(Consequence, block.Consequence);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Condition, Consequence);
    }
}

public class ReplaceBlock : Block
{
    public string Property { get; set; } = default!;

    public override bool Equals(object? obj)
    {
        return obj is ReplaceBlock block &&
               Property == block.Property;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Property);
    }
}
