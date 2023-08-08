using DotnetMailMerge;

namespace DotnetMailMerge.Markdown;

public class Parser
{
	private readonly Lexer _lexer;
    private Token _curToken;

	public Parser(Lexer lexer)
	{
		_lexer = lexer;
		_curToken = _lexer.GetNextToken();
	}

	public Result<Ast> Parse()
	{ 
        var blocks = new List<IBlock>();

		IBlock block = _curToken.TokenType switch {
			TokenType.Heading => ParseHeading(),
			TokenType.Letter => ParseParagraph(),
            _ => throw new NotImplementedException(),
        };

		blocks.Add(block);

		return new Ast { Blocks = blocks };
    }

	private ParagraphBlock ParseParagraph()
	{
		var str = _curToken.Literal;
		_curToken = _lexer.GetNextToken();

		while (_curToken.TokenType is not TokenType.EOF)
		{
			str += _curToken.Literal;
            _curToken = _lexer.GetNextToken();
        }

		return new() { Text = str };
    }

	private HeadingBlock ParseHeading()
	{
		_curToken = _lexer.GetNextToken();

		var level = 1;
		while (_curToken.TokenType is TokenType.Heading)
		{
			level++;
			_curToken = _lexer.GetNextToken();
        }

		var str = "";
		while (_curToken.TokenType is TokenType.Letter)
		{
			str += _curToken.Literal;
			_curToken = _lexer.GetNextToken();
        }
		return new HeadingBlock(level, str);
    }
}

public class Ast 
{
    public List<IBlock> Blocks { get; set; } = default!;
}

public interface IBlock { }

public class HeadingBlock : IBlock
{
    public readonly string Text;
    public readonly int Level;
	public HeadingBlock(int level, string text)
	{
		Text = text;
		Level = level;
    }
}

public class ParagraphBlock : IBlock
{
	public string Text { get; set; }

}
