using DotnetMailMerge.Markdown;
using NUnit.Framework;

namespace DotnetMailMerge.Tests.Markdown;

[TestFixture]
public class MarkdownRendererTests
{
    [TestCase("Lorem ipsum", "<p>Lorem ipsum</p>")]
    [TestCase("# Heading", "<h1>Heading</h1>")]
    [TestCase("# Heading\nLorem ipsum", "<h1>Heading</h1>\n<p>Lorem ipsum</p>")]
    [TestCase("* A", "<ul><li>A</li></ul>")]
    public void TestRender(string input, string expected)
    {
        var renderer = GetRenderer(input);

        var result = renderer.Render();

        Assert.That(result, Is.EqualTo(expected));
    }

    private Renderer GetRenderer(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var renderer = new Renderer(parser);

        return renderer;
    }
}

