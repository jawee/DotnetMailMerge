using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DotnetMailMerge.Tests;

[TestFixture]
public class ParserTests
{
    [Test]
    public void TestParseTextBlock()
    {
        var input = "<html></html>";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var ast = parser.Parse().GetValue();

        if (ast.Blocks.Count != 1)
        {
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var textBlock = ast.Blocks.First() as TextBlock;
        if (textBlock is null)
        { 
            Assert.Fail("Expected 'TextBlock', got '{0}'", ast.Blocks.First().GetType());
        }

        Assert.That(textBlock, Is.Not.Null);
        Assert.That(textBlock.Text, Is.EqualTo(input));

    }

    [Test]
    public void TestParseReplacementBlock_NoWhitespace()
    {
        var input = "{{myproperty}}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var ast = parser.Parse().GetValue();

        if (ast.Blocks.Count != 1)
        {
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var replaceBlock = ast.Blocks.First() as ReplaceBlock;
        if (replaceBlock is null)
        { 
            Assert.Fail("Expected 'ReplaceBlock', got '{0}'", ast.Blocks.First().GetType());
        }

        Assert.That(replaceBlock, Is.Not.Null);
        Assert.That(replaceBlock.Property, Is.EqualTo("myproperty"));
    }

    [Test]
    public void TestParseReplacementBlock()
    {
        var input = "{{ myproperty }}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var ast = parser.Parse().GetValue();

        if (ast.Blocks.Count != 1)
        {
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var replaceBlock = ast.Blocks.First() as ReplaceBlock;

        Assert.That(replaceBlock, Is.Not.Null, "Expected 'ReplaceBlock', got '{0}'", ast.Blocks.First().GetType());
        Assert.That(replaceBlock.Property, Is.EqualTo("myproperty"));
    }

    [Test]
    public void TestParseReplacementBlockNested()
    {
        var expectedBlocks = new List<Block>
        {
            new TextBlock { Text = "<p>"},
            new ReplaceBlock { Property = "myproperty"},
            new TextBlock { Text = "</p>"},
        };
        var input = "<p>{{ myproperty }}</p>";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var ast = parser.Parse().GetValue();

        if (ast.Blocks.Count != 3)
        {
            Assert.Fail("Expected '3' Block, got '{0}'", ast.Blocks.Count);
        }

        for (var i = 0; i < expectedBlocks.Count; ++i)
        {
            Assert.That(ast.Blocks[i], Is.EqualTo(expectedBlocks[i]));
        }
    }

    [Test]
    public void TestParseReplacementBlockTwoNested()
    {
        var expectedBlocks = new List<Block>
        {
            new TextBlock { Text = "<p>"},
            new ReplaceBlock { Property = "prop1"},
            new ReplaceBlock { Property = "prop2"},
            new TextBlock { Text = "</p>"},
        };
        var input = "<p>{{ prop1 }}{{ prop2 }}</p>";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var ast = parser.Parse().GetValue();

        if (ast.Blocks.Count != 4)
        {
            Assert.Fail("Expected '4' Block, got '{0}'", ast.Blocks.Count);
        }

        for (var i = 0; i < expectedBlocks.Count; ++i)
        {
            Assert.That(ast.Blocks[i], Is.EqualTo(expectedBlocks[i]));
        }
    }
    [Test]
    public void TestParseIfBlock()
    {
        var input = "{{#if somebool }}{{/if}}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var ast = parser.Parse().GetValue();

        if (ast.Blocks.Count != 1)
        {
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var ifBlock = ast.Blocks.First() as IfBlock;

        Assert.That(ifBlock, Is.Not.Null, "Expected 'IfBlock', got '{0}'", ast.Blocks.First().GetType());
        Assert.That(ifBlock.Condition, Is.EqualTo("somebool"));
        Assert.That(ifBlock.Consequence, Has.Count.EqualTo(0));
    }

    [Test]
    public void TestParseIfBlockWithTextConsequence()
    {
        var input = "{{#if somebool }}Lorem ipsum{{/if}}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var ast = parser.Parse().GetValue();

        if (ast.Blocks.Count != 1)
        {
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var ifBlock = ast.Blocks.First() as IfBlock;

        Assert.That(ifBlock, Is.Not.Null, "Expected 'IfBlock', got '{0}'", ast.Blocks.First().GetType());
        Assert.That(ifBlock.Condition, Is.EqualTo("somebool"));
        Assert.That(ifBlock.Consequence, Has.Count.EqualTo(1));
        var consequenceBlock = ifBlock.Consequence.First() as TextBlock;
        Assert.That(consequenceBlock, Is.Not.Null, "Expected 'TextBlock', got '{0}'", ast.Blocks.First().GetType());
        Assert.That(consequenceBlock.Text, Is.EqualTo("Lorem ipsum"));
    }

    [Test]
    public void TestParseNestedIf()
    {
        var input = "{{#if somebool }}{{#if otherbool}}Lorem ipsum{{/if}}{{/if}}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var ast = parser.Parse().GetValue();

        if (ast.Blocks.Count != 1)
        {
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var ifBlock = ast.Blocks.First() as IfBlock;

        Assert.That(ifBlock, Is.Not.Null, "Expected 'IfBlock', got '{0}'", ast.Blocks.First().GetType());
        Assert.That(ifBlock.Condition, Is.EqualTo("somebool"));
        Assert.That(ifBlock.Consequence, Has.Count.EqualTo(1));
        var consequenceBlock = ifBlock.Consequence.First() as IfBlock;
        Assert.That(consequenceBlock, Is.Not.Null, "Expected 'IfBlock', got '{0}'", ifBlock.Consequence.First().GetType());
        Assert.That(consequenceBlock.Condition, Is.EqualTo("otherbool"));
        Assert.That(consequenceBlock.Consequence, Has.Count.EqualTo(1));
        var consequence = consequenceBlock.Consequence.First() as TextBlock;
        Assert.That(consequence, Is.Not.Null, "Expected 'TextBlock', got '{0}'", consequenceBlock.Consequence.First().GetType());
        Assert.That(consequence.Text, Is.EqualTo("Lorem ipsum"));
    }

    [Test]
    public void TestParseIfBlockWithTextAndReplaceBlockConsequence()
    {
        var expectedConsequence = new List<Block>
        {
            new TextBlock { Text = "<p>"},
            new ReplaceBlock { Property = "p"},
            new TextBlock { Text = "</p>"},
        };
        var input = "{{#if somebool }}<p>{{ p }}</p>{{/if}}";
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var ast = parser.Parse().GetValue();

        if (ast.Blocks.Count != 1)
        {
            Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
        }

        var ifBlock = ast.Blocks.First() as IfBlock;

        Assert.That(ifBlock, Is.Not.Null, "Expected 'IfBlock', got '{0}'", ast.Blocks.First().GetType());
        Assert.That(ifBlock.Condition, Is.EqualTo("somebool"));
        Assert.That(ifBlock.Consequence, Has.Count.EqualTo(3));

        for (var i = 0; i < expectedConsequence.Count; ++i)
        {
            Assert.That(ifBlock.Consequence[i], Is.EqualTo(expectedConsequence[i]));
        }
    }

    //[Test]
    //public void TestParseIfElse() 
    //{ 
    //    var input = "{{#if somebool }}Lorem ipsum{{else}}Ipsum lorem{{/if}}";
    //    var lexer = new Lexer(input);
    //    var parser = new Parser(lexer);
    //    var ast = parser.Parse().GetValue();

    //    if (ast.Blocks.Count != 1)
    //    {
    //        Assert.Fail("Expected '1' Block, got '{0}'", ast.Blocks.Count);
    //    }

    //    var ifBlock = ast.Blocks.First() as IfBlock;

    //    Assert.That(ifBlock, Is.Not.Null, "Expected 'IfBlock', got '{0}'", ast.Blocks.First().GetType());
    //    Assert.That(ifBlock.Condition, Is.EqualTo("somebool"));
    //    Assert.That(ifBlock.Consequence, Has.Count.EqualTo(1));
    //    var consequenceBlock = ifBlock.Consequence.First() as TextBlock;
    //    Assert.That(consequenceBlock, Is.Not.Null, "Expected 'TextBlock', got '{0}'", ast.Blocks.First().GetType());
    //    Assert.That(consequenceBlock.Text, Is.EqualTo("Lorem ipsum"));

    //    Assert.That(ifBlock.Alternative, Has.Count.EqualTo(1));
    //    var alternativeBlock = ifBlock.Alternative.First() as TextBlock;
    //    Assert.That(alternativeBlock, Is.Not.Null, "Expected 'TextBlock', got '{0}'", ast.Blocks.First().GetType());
    //    Assert.That(alternativeBlock.Text, Is.EqualTo("Ipsum lorem"));
    //}
}
