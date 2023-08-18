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

		while (_curToken.TokenType != TokenType.EOF)
		{
			IBlock block = _curToken.TokenType switch
			{
				TokenType.Heading => ParseHeading(),
				TokenType.Letter => ParseParagraph(),
				TokenType.Item => ParseItem(),
				_ => throw new NotImplementedException(),
			};

			blocks.Add(block);
			_curToken = _lexer.GetNextToken();
		}

		return new Ast { Blocks = blocks };
	}

	private ItemBlock ParseItem()
	{
		_curToken = _lexer.GetNextToken();

		var str = "";
		while (_curToken.TokenType is TokenType.Letter)
		{
			str += _curToken.Literal;
			_curToken = _lexer.GetNextToken();
		}

		if (_curToken.TokenType is TokenType.LineBreak)
		{

			if (_lexer.PeekNextToken().TokenType is TokenType.LineBreak)
			{
				// double line break, should get next block
				_curToken = _lexer.GetNextToken();
			}
			else if (_lexer.PeekNextToken().TokenType is TokenType.Item)
			{
				//do nothing
			}
			else {
				//still in item, should just parse until end
				str += "\n";
				_curToken = _lexer.GetNextToken();
				while (_curToken.TokenType is TokenType.Letter)
				{
					str += _curToken.Literal;
					_curToken = _lexer.GetNextToken();
				}
				_curToken = _lexer.GetNextToken();
			}
		}
		return new ItemBlock(str);
	}

    private bool ShouldContinueReadingParagraph()
    {
        if (_curToken.TokenType is TokenType.EOF)
        {
			return false;
        }
        if (_curToken.TokenType is TokenType.LineBreak && _lexer.PeekNextToken().TokenType is TokenType.LineBreak)
        {
			_curToken = _lexer.GetNextToken();
			return false;
        }

		return true;
    }

    private ParagraphBlock ParseParagraph()
	{
		var str = _curToken.Literal;
		_curToken = _lexer.GetNextToken();

		while (ShouldContinueReadingParagraph())
		{
			if (_curToken.TokenType is TokenType.LineBreak)
			{
				str += " ";
            }
			else 
            {
                str += _curToken.Literal;
            }
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

public class ItemBlock : IBlock
{
	public string Text { get; set; }

	public ItemBlock(string text)
	{
		Text = text;
	}
}
