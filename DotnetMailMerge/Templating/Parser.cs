using DotnetMailMerge.Exceptions;

namespace DotnetMailMerge.Templating;

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
            if (block is null)
            {
                return blockResult.GetError();
            }
            blocks.Add(block);
        }

        return new Ast { Blocks = blocks };
    }

    private Result<Block> ParseBlock() 
    {
        return _curToken.TokenType switch
        {
            TokenType.Start => ParseLogicBlock(),
            TokenType.StartMd => ParseMdReplaceBlock(),
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

        while (ConsequenceShouldReadNext())
        {
            var blockResult = ParseBlock();

            var block = blockResult.Match(success => success, _ => null);
            if (block is null)
            {
                throw new Exception("Exception in ParseConsequence");
            }
            blocks.Add(block);
        }

        return blocks;
    }

    private bool CheckLiteral(string c, string[] words, int pos)
    {
        foreach(var word in words)
        {
            if (c != $"{word[pos]}")
            {
                return false;
            }
        }

        return true;
    }
    private bool ConsequenceShouldReadNext()
    {
        if (_curToken.TokenType is TokenType.Start && _peekToken.Literal == "/")
        {
            return false;
        }

        if (_curToken.TokenType is TokenType.Start && _peekToken.Literal == "e")
        {
            var matching = new string[] { "lse", "lseif" };
            var c = 0;
            var peekToken = _lexer.PeekNthToken(_lexer.GetReadPos());
            while (peekToken.TokenType is TokenType.Character && CheckLiteral(peekToken.Literal, matching, c))
            {
                peekToken = _lexer.PeekNthToken(_lexer.GetReadPos()+(c++));
            }
            return false;
        }

        return true;
    }

    private string ParseConditional() 
    {
        NextToken(); // #
        var conditional = "";
        while (_curToken.Literal != " ")
        {
            conditional += _curToken.Literal;
            NextToken();
        }

        return conditional.Trim();
    }

    private Result<Block> ParseIf()
    {
        //if asdf }} consequence {{/if}}
        var conditional = ParseConditional();

        if (conditional is not "if")
        { 
            return new UnknownConditionalException($"Conditional was not 'if', was '{conditional}'");
        }

        NextToken();

        var condition = ParseCondition();

        var consequence = ParseConsequence();

        if (_curToken.TokenType != TokenType.Start)
        {
            return new Exception($"Not sure this should happen. {_curToken.TokenType} {_curToken.Literal}");
        }

        NextToken();
        //TODO: Check if it's actual end or {{else}} {{elseif}}
        var nextConditional = ParseCondition();

        var alternative = new List<Block>();
        if (nextConditional is not "/if") 
        {
            alternative = nextConditional switch
            {
                "else" => ParseConsequence(),
                _ => throw new Exception($"'{nextConditional}'"),
            };
        }

        _ = ParseCondition();
        NextToken();

        return new IfBlock { Condition = condition.Trim(), Consequence = consequence , Alternative = alternative };
    }

    private string ParseCondition()
    {
        var condition = "";
        while (_curToken.TokenType != TokenType.End)
        {
            condition += _curToken.Literal;
            NextToken();
        }

        return condition;
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

    private Result<Block> ParseMdReplaceBlock()
    {
        var res = "";

        while (_curToken.TokenType != TokenType.EndMd)
        {
            res += _curToken.Literal;
            NextToken();
        }

        NextToken();

        return new MdReplaceBlock() { Content = res.Trim() };
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

public class MdReplaceBlock : Block
{
    public string Content { get; set; } = default!;

    public override bool Equals(object? obj)
    {
        return obj is MdReplaceBlock block &&
               Content == block.Content;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Content);
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
