namespace DotnetMailMerge.Templating;

public class Lexer
{
	private readonly string _input;
	private char? _currentChar;
	private int _readPos;

	public Lexer(string input)
	{
		_input = input;
		_readPos = 0;
		_currentChar = input.Length == 0 ? null : _input[0];
	}

	public Token PeekNthToken(int n)
	{
		var c = PeekNthChar(n);

		var res = c switch
		{
			'{' => PeekStart(n),
			'}' => PeekEnd(n),
			null => new Token(TokenType.EOF),
			_ => new Token(TokenType.Character, $"{c}"),
		};

		return res;
	}

	public int GetReadPos()
	{
		return _readPos;
	}

	private Token PeekStart(int n)
	{
		var c = PeekNthChar(n+1);
		var res = c switch
		{
			var a when PeekIsMdStart(a, n+1) => new Token(TokenType.StartMd),
			'{' => new Token(TokenType.Start),
			_ => new Token(TokenType.Character, "{"),
		};

		return res;
	}

	private bool PeekIsMdStart(char? a, int readPos)
	{
		var peekChar = PeekNthChar(readPos + 1);
		if (a is not '{' || peekChar is not '{')
		{
			return false;
        }

		return true;
    }

	private bool PeekIsMdEnd(char? a, int readPos)
	{
		var peekChar = PeekNthChar(readPos + 1);
		if (a is not '}' || peekChar is not '}')
		{
			return false;
        }

		return true;
    }

	private Token PeekEnd(int n)
	{
		var c = PeekNthChar(n+1);
		var res = c switch
		{
			var a when PeekIsMdEnd(a, n+1) => new Token(TokenType.EndMd),
			'}' => new Token(TokenType.End),
			_ => new Token(TokenType.Character, "}"),
		};

		return res;
	}

	public Token GetNextToken()
	{
        var res = _currentChar switch
        {
            '{' => ReadStart(),
			'}' => ReadEnd(),
            null => new Token(TokenType.EOF),
			_ => new Token(TokenType.Character, $"{_currentChar}"),
        };

		ReadNextChar();
		return res;
    }

	private Token ReadEnd() 
    { 
		ReadNextChar();
		var res = _currentChar switch
		{
			var a when IsMdEnd(a) => new Token(TokenType.EndMd),
			'}' => new Token(TokenType.End),
			_ => new Token(TokenType.Character, "}"),
		};

		return res;
    }

	private Token ReadStart() 
    {
		ReadNextChar();
		var res = _currentChar switch
		{
			var a when IsMdStart(a) => new Token(TokenType.StartMd),
			'{' => new Token(TokenType.Start),
			_ => new Token(TokenType.Character, "{"),
		};

		return res;
    }

	private bool IsMdStart(char? a)
	{
		var peekChar = PeekNthChar(_readPos + 1);
		if (a is not '{' || peekChar is not '{')
		{
			return false;
        }

		ReadNextChar();

		return true;
    }

	private bool IsMdEnd(char? a)
	{
		var peekChar = PeekNthChar(_readPos + 1);
		if (a is not '}' || peekChar is not '}')
		{
			return false;
        }

		ReadNextChar();

		return true;
    }

	private char? PeekNthChar(int n)
	{
		return _input.Length > n ? _input[n] : null;
    }

	private void ReadNextChar()
	{
		_readPos++;
		_currentChar = _input.Length > _readPos ? _input[_readPos] : null;
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
	Character,
	Start,
	End,
	EOF,
	Illegal,
    StartMd,
    EndMd,
}
