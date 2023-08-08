using System.Linq;
using DotnetMailMerge.Markdown;
using NUnit.Framework;

namespace DotnetMailMerge.Tests;

[TestFixture]
public class MarkdownParserTests
{
    [Test]
    public void TestParseTextBlock()
    {
        var input = "Lorem ipsum dolor sit amet.";
        var ast = GetAst(input);

        if (ast.Blocks.Count != 1)
        {
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var paragraphBlock = ast.Blocks.First() as ParagraphBlock;
        if (paragraphBlock is null)
        {
            Assert.Fail("Expected 'TextBlock', got '{0}'", ast.Blocks.First().GetType());
        }

        Assert.That(paragraphBlock, Is.Not.Null);
        Assert.That(paragraphBlock.Text, Is.EqualTo(input));
    }

    [Test]
    public void TestParseHeadingBlock()
    {
        var input = "# Lorem";
        var ast = GetAst(input);

        if (ast.Blocks.Count != 1)
        { 
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var headingBlock = ast.Blocks.First() as HeadingBlock;
        if (headingBlock is null)
        {
            Assert.Fail("Expected 'TextBlock', got '{0}'", ast.Blocks.First().GetType());
        }

        Assert.That(headingBlock, Is.Not.Null);
        Assert.That(headingBlock.Text, Is.EqualTo("Lorem"));
        Assert.That(headingBlock.Level, Is.EqualTo(1));
    }

    private static Ast GetAst(string input)
    { 
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var parseResult = parser.Parse();

        if (parseResult.IsError)
        {
            Assert.Fail($"{parseResult.GetError()}");
        }
        var ast = parseResult.GetValue();

        return ast;
    }
}

