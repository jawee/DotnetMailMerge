using DotnetMailMerge.Markdown;
using NUnit.Framework;

namespace DotnetMailMerge.Tests;

[TestFixture]
public class MarkdownLexerTests
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
            new TestCase("", new[] { CreateToken(TokenType.EOF)}),
            new TestCase("# ", new[] { CreateToken(TokenType.Heading) }),
            new TestCase("*.", new[] { CreateToken(TokenType.Item)} ),
            new TestCase("\n", new[] { CreateToken(TokenType.LineBreak)}),
            new TestCase("a", new[] { CreateToken(TokenType.Letter, "a")}),
            new TestCase("## ", new[] { CreateToken(TokenType.Heading), CreateToken(TokenType.Heading)}),
            new TestCase("# a#", new[] {
                CreateToken(TokenType.Heading),
                CreateToken(TokenType.Letter, "a"),
                CreateToken(TokenType.Letter, "#")
            }),
            new TestCase("as df", new[] {
                CreateToken(TokenType.Letter, "a"),
                CreateToken(TokenType.Letter, "s"),
                CreateToken(TokenType.Letter, " "),
                CreateToken(TokenType.Letter, "d"),
                CreateToken(TokenType.Letter, "f"),
            }),
        };

        foreach (var testCase in testCases)
        {
            var lexer = new Lexer(testCase.Input);
            foreach (var expected in testCase.ExpectedTokens)
            {
                var token = lexer.GetNextToken();
                Assert.Multiple(() =>
                {
                    Assert.That(token.Literal, Is.EqualTo(expected.Literal), "TestCase: '{0}'. Expected '{1}', got '{2}'", testCase.Input, expected.Literal, token.Literal);
                    Assert.That(token.TokenType, Is.EqualTo(expected.TokenType), "TestCase: '{0}'. Expected '{1}, got '{2}'", testCase.Input, expected.TokenType, token.TokenType);
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
