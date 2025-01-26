using DotnetMailMerge.Templating;
using NUnit.Framework;

namespace DotnetMailMerge.Tests.Templating;

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
            new TestCase("{", [CreateToken(TokenType.Character, "{")]),
            new TestCase("}", [CreateToken(TokenType.Character, "}")]),
            new TestCase("{{", [CreateToken(TokenType.Start), CreateToken(TokenType.EOF)]),
            new TestCase("}}", [CreateToken(TokenType.End), CreateToken(TokenType.EOF)]),
            new TestCase("{{{", [CreateToken(TokenType.StartMd)]),
            new TestCase("}}}", [CreateToken(TokenType.EndMd)]),
            new TestCase("{{p}}", [
                CreateToken(TokenType.Start),
                CreateToken(TokenType.Character, "p"),
                CreateToken(TokenType.End),
                CreateToken(TokenType.EOF),
            ]),
            new TestCase("{{#if p}}", [
                CreateToken(TokenType.Start),
                CreateToken(TokenType.Character, "#"),
                CreateToken(TokenType.Character, "i"),
                CreateToken(TokenType.Character, "f"),
                CreateToken(TokenType.Character, " "),
                CreateToken(TokenType.Character, "p"),
                CreateToken(TokenType.End),
                CreateToken(TokenType.EOF),
            ]),
            new TestCase("<p>{{ p }}</p>", [
                CreateToken(TokenType.Character, "<"),
                CreateToken(TokenType.Character, "p"),
                CreateToken(TokenType.Character, ">"),
                CreateToken(TokenType.Start),
                CreateToken(TokenType.Character, " "),
                CreateToken(TokenType.Character, "p"),
                CreateToken(TokenType.Character, " "),
                CreateToken(TokenType.End),
                CreateToken(TokenType.Character, "<"),
                CreateToken(TokenType.Character, "/"),
                CreateToken(TokenType.Character, "p"),
                CreateToken(TokenType.Character, ">"),
            ]),
            new TestCase("{{{ a }}}", [
                CreateToken(TokenType.StartMd),
                CreateToken(TokenType.Character, " "),
                CreateToken(TokenType.Character, "a"),
                CreateToken(TokenType.Character, " "),
                CreateToken(TokenType.EndMd),
            ]),
        };

        foreach (var testCase in testCases)
        {
            var lexer = new Lexer(testCase.Input);
            foreach (var expected in testCase.ExpectedTokens)
            {
                var token = lexer.GetNextToken();
                Assert.Multiple(() =>
                {
                    Assert.That(token.Literal, Is.EqualTo(expected.Literal),string.Format("TestCase: '{0}'. Expected '{1}', got '{2}'", testCase.Input, expected.Literal, token.Literal));
                    Assert.That(token.TokenType, Is.EqualTo(expected.TokenType), string.Format("TestCase: '{0}'. Expected '{1}, got '{2}'", testCase.Input, expected.TokenType, token.TokenType));
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

