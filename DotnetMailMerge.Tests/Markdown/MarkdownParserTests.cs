using System.Linq;
using DotnetMailMerge.Markdown;
using NUnit.Framework;

namespace DotnetMailMerge.Tests;

[TestFixture]
public class MarkdownParserTests
{

    [Test]
    public void TestParseHeadingItems()
    {
        var input = "# Heading\n* A\n* B";
        var expected = new IBlock[] { 
            new HeadingBlock(1, "Heading"),
            new ItemBlock("A"),
            new ItemBlock("B"),
        };

        var ast = GetAst(input);
        if (ast.Blocks.Count != 3)
        { 
            Assert.Fail("Expected '3' Block, got '{0}'", ast.Blocks.Count);
        }

        var block = ast.Blocks[0] as HeadingBlock;
        if (block is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(HeadingBlock), ast.Blocks.First().GetType());
        }
        Assert.That(block, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(block.Text, Is.EqualTo("Heading"));
            Assert.That(block.Level, Is.EqualTo(1));
        });

        var itemBlock = ast.Blocks[1] as ItemBlock;
        if (itemBlock is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(ItemBlock), ast.Blocks.First().GetType());
        }
        Assert.That(itemBlock, Is.Not.Null);
        Assert.That(itemBlock.Text, Is.EqualTo("A"));

        itemBlock = ast.Blocks[2] as ItemBlock;
        if (itemBlock is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(ItemBlock), ast.Blocks.First().GetType());
        }
        Assert.That(itemBlock, Is.Not.Null);
        Assert.That(itemBlock.Text, Is.EqualTo("B"));
    }

    [Test]
    public void TestParseTwoItems()
    { 
        var input = "* A\n* B";
        var ast = GetAst(input);

        if (ast.Blocks.Count != 2)
        {
            Assert.Fail("Expected '2' Block, got '{0}'", ast.Blocks.Count);
        }

        var itemBlock = ast.Blocks.First() as ItemBlock;
        if (itemBlock is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(ItemBlock), ast.Blocks.First().GetType());
        }

        Assert.That(itemBlock, Is.Not.Null);
        Assert.That(itemBlock.Text, Is.EqualTo("A"));

        itemBlock = ast.Blocks.Last() as ItemBlock;
        if (itemBlock is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(ItemBlock), ast.Blocks.Last().GetType());
        }

        Assert.That(itemBlock, Is.Not.Null);
        Assert.That(itemBlock.Text, Is.EqualTo("B"));

    }

    [Test]
    public void TestParseItem()
    { 
        var input = "* A";
        var ast = GetAst(input);

        if (ast.Blocks.Count != 1)
        {
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var itemBlock = ast.Blocks.First() as ItemBlock;
        if (itemBlock is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(ItemBlock), ast.Blocks.First().GetType());
        }

        Assert.That(itemBlock, Is.Not.Null);
        Assert.That(itemBlock.Text, Is.EqualTo("A"));
    }

    [Test]
    public void TestParseItemParagraphBlocks()
    {
        var input = "* A\n\nLorem ipsum.";
        var ast = GetAst(input);

        if (ast.Blocks.Count != 2)
        {
            Assert.Fail("Expected '2' Block, got '{0}'", ast.Blocks.Count);
        }

        var itemBlock = ast.Blocks.First() as ItemBlock;
        if (itemBlock is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(ItemBlock), ast.Blocks.First().GetType());
        }

        Assert.That(itemBlock, Is.Not.Null);
        Assert.That(itemBlock.Text, Is.EqualTo("A"));

        var paragraphBlock = ast.Blocks.Last() as ParagraphBlock;
        if (paragraphBlock is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(ParagraphBlock), ast.Blocks.Last().GetType());
        }
        Assert.That(paragraphBlock, Is.Not.Null);
        Assert.That(paragraphBlock.Text, Is.EqualTo("Lorem ipsum."));
    }

    [Test]
    public void TestParseHeadingAndTextBlock()
    {
        var input = "# Heading\nLorem ipsum dolor sit amet.";
        var ast = GetAst(input);

        if (ast.Blocks.Count != 2)
        {
            Assert.Fail("Expected '2' Block, got '{0}'", ast.Blocks.Count);
        }

        var headingBlock = ast.Blocks.First() as HeadingBlock;
        if (headingBlock is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(HeadingBlock), ast.Blocks.First().GetType());
        }

        Assert.That(headingBlock, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(headingBlock.Text, Is.EqualTo("Heading"));
            Assert.That(headingBlock.Level, Is.EqualTo(1));
        });
        var paragraphBlock = ast.Blocks.Last() as ParagraphBlock;
        if (paragraphBlock is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(ParagraphBlock), ast.Blocks.Last().GetType());
        }
        Assert.That(paragraphBlock, Is.Not.Null);
        Assert.That(paragraphBlock.Text, Is.EqualTo("Lorem ipsum dolor sit amet."));
    }

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
            Assert.Fail("Expected '{0}', got '{1}'", nameof(ParagraphBlock), ast.Blocks.First().GetType());
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
            Assert.Fail("Expected '{0}', got '{1}'", nameof(HeadingBlock), ast.Blocks.First().GetType());
        }

        Assert.That(headingBlock, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(headingBlock.Text, Is.EqualTo("Lorem"));
            Assert.That(headingBlock.Level, Is.EqualTo(1));
        });
    }

    [Test]
    public void TestParseHeadingLevelTwoBlock()
    {
        var input = "## Lorem";
        var ast = GetAst(input);

        if (ast.Blocks.Count != 1)
        { 
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var headingBlock = ast.Blocks.First() as HeadingBlock;
        if (headingBlock is null)
        {
            Assert.Fail("Expected '{0}', got '{1}'", nameof(HeadingBlock), ast.Blocks.First().GetType());
        }

        Assert.That(headingBlock, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(headingBlock.Text, Is.EqualTo("Lorem"));
            Assert.That(headingBlock.Level, Is.EqualTo(2));
        });
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

