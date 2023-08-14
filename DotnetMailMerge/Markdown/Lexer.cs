using System.Linq;

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
		_prevChar = _currentChar;
		_readPos++;
		_currentChar = _input.Length > _readPos ? _input[_readPos] : null;
    }

    public Token GetNextToken()
	{
		ReadNextChar();

		char? next = _input.Length > _readPos+1 ? _input[_readPos+1] : null; 
		var token = _currentChar switch { 
			var a when IsHeading(a, _prevChar, next) => LexHeading(),
			var a when IsItem(a, _prevChar, next) => LexItem(),
			'\n' => new Token(TokenType.LineBreak),
			null => new Token(TokenType.EOF),
			_ => new Token(TokenType.Letter, _currentChar.ToString()),
        };

        return token;
    }

	public Token PeekNextToken()
	{ 
		char? peekChar = _input.Length > _readPos+1 ? _input[_readPos+1] : null; 
		char? next = _input.Length > _readPos+2 ? _input[_readPos+2] : null; 

		var token = peekChar switch { 
			var a when IsHeading(a, _currentChar, next) => new Token(TokenType.Heading),
			var a when IsItem(a, _currentChar, next) => new Token(TokenType.Item),
			'\n' => new Token(TokenType.LineBreak),
			null => new Token(TokenType.EOF),
			_ => new Token(TokenType.Letter, _currentChar.ToString()),
        };

		return token;
    }

	private Token LexItem()
	{
        ReadNextChar();
        return new Token(TokenType.Item);
    }

	private Token LexHeading()
	{
		if (_input[_readPos+1] == ' ')
		{
            ReadNextChar();
        }
		return new Token(TokenType.Heading);
    }

	private bool IsItem(char? a, char? prev, char? next)
	{
		return IsSpecial(a, prev, next, '*', new char?[] { '\n', null }, new char?[] { ' ' });
    }

	private bool IsHeading(char? a, char? prev, char? next)
	{
		return IsSpecial(a, prev, next, '#', new char?[] { '#', '\n', null}, new char?[] { '#', ' ' });
    }

	private bool IsSpecial(char? a, char? prev, char? next, char? expected, char?[] allowedPrev, char?[] allowedNext)
	{
		if (a != expected)
		{
			return false;
		}

		if (_input.Length > _readPos+1 && allowedNext.Contains(next) && allowedPrev.Contains(prev))
		{
			return true;
        }
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



