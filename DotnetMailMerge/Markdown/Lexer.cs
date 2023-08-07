namespace DotnetMailMerge.Markdown;

public class Lexer
{
	private readonly string _input;
	private char? _currentChar;
	private int _readPos;
	private char? _prevChar;

	public Lexer(string input)
	{
		_input = input;
		_readPos = -1;
		_currentChar = null;
		_prevChar = null;
	}

	private void ReadNextChar()
	{
		_readPos++;
		_currentChar = _input.Length > _readPos ? _input[_readPos] : null;
    }

    public Token GetNextToken()
	{
		ReadNextChar();

		var token = _currentChar switch { 
			var a when (a == '#' && IsSpecial(a, '#')) => new Token(TokenType.Heading),
			var a when (a == '*' && IsSpecial(a, null)) => new Token(TokenType.Item),
			'\n' => new Token(TokenType.LineBreak),
			_ => new Token(TokenType.Letter, _currentChar.ToString()),
        };

		return token;
    }

	private bool IsSpecial(char? a, char? allowedPrev)
	{
		if ( _prevChar == allowedPrev || _prevChar == null || _prevChar == '\n')
		{
			return true;
        }
		return false;
    }

	private bool IsAllowed(char? a)
	{
		return false;
    }
}

public readonly struct Token 
{ 
	public readonly TokenType TokenType { get; }
	public readonly string? Literal { get; }
	public Token(TokenType tokenType, string? literal = null)
	{
		TokenType = tokenType;
		Literal = literal;
    }
}

public enum TokenType
{
    EOF,
    Illegal,
    Heading,
    Letter,
    LineBreak,
    //OrderedItem,
    Item,
    LParen,
    RParen,
    RBracket,
    LBracket,
    Bang,
}



