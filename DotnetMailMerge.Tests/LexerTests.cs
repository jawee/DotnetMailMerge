using NUnit.Framework;

namespace DotnetMailMerge.Tests;

[TestFixture]
public class LexerTests
{
    [Test]
    public void TestInitializeLexer_Ok()
    {
        var input = "";
        _ = new Lexer(input);
    }

    [Test]
    public void TestLexer() 
    {
        var testCases = new[] {
            new TestCase("{", new[] { CreateToken(TokenType.Start, "{") }),
            new TestCase("}", new[] { CreateToken(TokenType.End, "}") }),
            new TestCase("{}", new[] { CreateToken(TokenType.Start, "{"), CreateToken(TokenType.End, "}")})
        };

        foreach (var testCase in testCases)
        {
            var lexer = new Lexer(testCase.Input);
            foreach (var expected in testCase.ExpectedTokens)
            {
                var token = lexer.GetNextToken();
                Assert.Multiple(() =>
                {
                    Assert.That(token.Literal, Is.EqualTo(expected.Literal),"Expected '{0}', got '{1}'", expected.Literal, token.Literal);
                    Assert.That(token.TokenType, Is.EqualTo(expected.TokenType), "Expected '{0}, got '{1}'", expected.TokenType, token.TokenType);
                });
            }
        }
    }

    private static Token CreateToken(TokenType tokenType, string? literal = null)
    {
        return new Token(tokenType, literal);
    }

    private readonly struct TestCase 
    {
        public readonly string Input { get; }
        public readonly Token[] ExpectedTokens { get; }

        public TestCase(string input, Token[] expectedToken)
        {
            Input = input;
            ExpectedTokens = expectedToken;
        }
    }
}

