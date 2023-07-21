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

    public Result<Ast> Parse()
    {
        var blocks = new List<Block>();

        while (_curToken.TokenType != TokenType.EOF)
        {
            var blockResult = ParseBlock();

            var block = blockResult.Match(success => success, _ => null);
            if (block is not null)
            {
                blocks.Add(block);
            }
            //NextToken();
        }

        return new Ast { Blocks = blocks };
    }

    private Result<Block> ParseBlock() 
    {
        return _curToken.TokenType switch
        {
            TokenType.Start => ParseLogicBlock(),
            TokenType.Character => ParseTextBlock(),
            _ => throw new Exception($"ParseBlock _ matched. {_curToken.TokenType} {_curToken.Literal}"),
        } ;
    }

    private Result<Block> ParseTextBlock()
    {
        var res = "";
        while (_curToken.TokenType is TokenType.Character)
        {
            res += _curToken.Literal;
            NextToken();
        }
        return new TextBlock { Text = res };
    }

    private List<Block> ParseConsequence()
    {
        NextToken();
        var blocks = new List<Block>();

        while (!(_curToken.TokenType == TokenType.Start && _peekToken.Literal == "/"))
        {
            var blockResult = ParseBlock();

            var block = blockResult.Match(success => success, _ => null);
            if (block is not null)
            {
                blocks.Add(block);
            }
            //NextToken();
        }

        return blocks;
    }

    private Result<string> ParseConditional() 
    {

        var conditional = "";
        while (_curToken.Literal != " ")
        {
            conditional = _curToken.Literal;
            NextToken();
        }

        return conditional.Trim();
    }

    private Result<Block> ParseIf()
    {
        //if asdf }} consequence {{/if}}
        var conditional = ParseConditional();
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
            return new Exception($"Not sure this should happen. {_curToken.TokenType} {_curToken.Literal}");
        }

        while (_curToken.TokenType != TokenType.End)
        {
            NextToken();
        }
        NextToken();

        return new IfBlock { Condition = condition.Trim(), Consequence = consequence };
    }

    private Result<Block> ParseReplacement()
    {
        var res = "";

        while (_curToken.TokenType != TokenType.End)
        {
            res += _curToken.Literal;
            NextToken();
        }

        NextToken();

        return new ReplaceBlock() { Property = res.Trim() };
    }

    private Result<Block> ParseLogicBlock()
    {
        NextToken();

        Result<Block> result = _curToken.Literal switch
        {
            "#" => ParseIf(),
            _ => ParseReplacement()
        };

        return result;
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
    public List<Block> Alternative { get; set; } = default!;

    public override bool Equals(object? obj)
    {
        return obj is IfBlock block &&
               Condition == block.Condition &&
               EqualityComparer<List<Block>>.Default.Equals(Consequence, block.Consequence) &&
               EqualityComparer<List<Block>>.Default.Equals(Alternative, block.Alternative);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Condition, Consequence, Alternative);
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
