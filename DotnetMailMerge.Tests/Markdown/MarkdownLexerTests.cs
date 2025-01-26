using DotnetMailMerge.Markdown;
using NUnit.Framework;

namespace DotnetMailMerge.Tests.Markdown;

[TestFixture]
public class MarkdownLexerTests
{
    [Test]
    public void TestLexer()
    {
        var testCases = new[] {
            new TestCase("", [CreateToken(TokenType.EOF)]),
            new TestCase("# ", [CreateToken(TokenType.Heading)]),
            new TestCase("* ", [CreateToken(TokenType.Item)] ),
            new TestCase("\n", [CreateToken(TokenType.LineBreak)]),
            new TestCase("a", [CreateToken(TokenType.Letter, "a")]),
            new TestCase("## ", [CreateToken(TokenType.Heading), CreateToken(TokenType.Heading)]),
            new TestCase("# a#", [
                CreateToken(TokenType.Heading),
                CreateToken(TokenType.Letter, "a"),
                CreateToken(TokenType.Letter, "#")
            ]),
            new TestCase("A B\n\nC D", [
                CreateToken(TokenType.Letter, "A"),
                CreateToken(TokenType.Letter, " "),
                CreateToken(TokenType.Letter, "B"),
                CreateToken(TokenType.LineBreak),
                CreateToken(TokenType.LineBreak),
                CreateToken(TokenType.Letter, "C"),
                CreateToken(TokenType.Letter, " "),
                CreateToken(TokenType.Letter, "D"),
            ]),
            new TestCase("as df", [
                CreateToken(TokenType.Letter, "a"),
                CreateToken(TokenType.Letter, "s"),
                CreateToken(TokenType.Letter, " "),
                CreateToken(TokenType.Letter, "d"),
                CreateToken(TokenType.Letter, "f"),
            ]),
            new TestCase("* A\n* B", [
                CreateToken(TokenType.Item),
                CreateToken(TokenType.Letter, "A"),
                CreateToken(TokenType.LineBreak),
                CreateToken(TokenType.Item),
                CreateToken(TokenType.Letter, "B"),
            ]),
            new TestCase("* A\nB", [
                CreateToken(TokenType.Item),
                CreateToken(TokenType.Letter, "A"),
                CreateToken(TokenType.LineBreak),
                CreateToken(TokenType.Letter, "B"),
                CreateToken(TokenType.EOF),
            ]),
            new TestCase("* A\n* B\nC\n", [
                CreateToken(TokenType.Item),
                CreateToken(TokenType.Letter, "A"),
                CreateToken(TokenType.LineBreak),
                CreateToken(TokenType.Item),
                CreateToken(TokenType.Letter, "B"),
                CreateToken(TokenType.LineBreak),
                CreateToken(TokenType.Letter, "C"),
                CreateToken(TokenType.LineBreak),
                CreateToken(TokenType.EOF),
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
                    Assert.That(token.Literal, Is.EqualTo(expected.Literal), string.Format("TestCase: '{0}'. Expected '{1}', got '{2}'", testCase.Input, expected.Literal, token.Literal));
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
