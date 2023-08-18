using NUnit.Framework;
using DotnetMailMerge.Exceptions;
using System;
using DotnetMailMerge.Templating;

namespace DotnetMailMerge.Tests;

public class MailMergeTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void MarkdownParagraphReplace()
    {
        var template = @"<body><div>{{{ paragraph }}}</div></body>";
        var expected = @"<body><div><p>Lorem ipsum</p></div></body>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "paragraph", "Lorem ipsum"} });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void MarkdownItemReplace()
    {
        var template = @"<body><div>{{{ items }}}</div></body>";
        var expected = @"<body><div><ul><li>A</li><li>B</li></ul></div></body>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "items", "* A\n* B"} });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }
    [Test]
    public void MarkdownReplaceHeadingAndParagraph()
    {
        var template = "<body><div>{{{ content }}}</div></body>";
        var expected = "<body><div><h1>Heading</h1><p>Lorem ipsum</p></div></body>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "content", "# Heading\nLorem ipsum"} });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void StringReplacement_SingleOccurrence()
    {
        var template = @"<html><body><h1>{{title}}</h1></body></html>";
        var expected = @"<html><body><h1>Title</h1></body></html>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "title", "Title" } });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void StringReplacement_MultipleOccurrence()
    {
        var template = @"<html><body><h1>{{title}}{{title}}{{title}}</h1></body></html>";
        var expected = @"<html><body><h1>TitleTitleTitle</h1></body></html>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "title", "Title" } });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }


    [Test]
    public void StringReplacement_MultipleParameters()
    {
        var template = @"<html><body><h1>{{title}}</h1><p>{{body}}</p></body></html>";
        var expected = @"<html><body><h1>Title</h1><p>Body</p></body></html>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { 
            { "title", "Title" },
            { "body", "Body" }
         });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void StringReplacement_NotAllReplaced_ReturnsError()
    {
        var template = @"<html><body><h1>{{title}}</h1><p>{{body}}</p></body></html>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "title", "Title" } });

        var error = result.Match(success => new Exception("Unexpected Success"), err => err);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsError, Is.True);
            Assert.That(error, Is.TypeOf<MissingParameterException>());
        });
    }

    [Test]
    public void If_Simple_ConditionTrue()
    {
        var template = @"<html><body><h1>{{title}}</h1><p>Lorem ipsum</p>{{#if show}}<p>Extra</p>{{/if}}</body></html>"; 
        var expected = @"<html><body><h1>Title</h1><p>Lorem ipsum</p><p>Extra</p></body></html>"; 

        var sut = new MailMerge(template);
        var result = sut.Render(new() { 
            { "title", "Title" },
            { "show", true }
         });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void If_Simple_ConditionFalse()
    {
        var template = @"<html><body><h1>{{title}}</h1><p>Lorem ipsum</p>{{#if show}}<p>Extra</p>{{/if}}</body></html>"; 
        var expected = @"<html><body><h1>Title</h1><p>Lorem ipsum</p></body></html>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() {
            { "title", "Title" },
            { "show", false }
         });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void If_EmptyStringCondition_EvaluatesAsFalse()
    { 
        var template = @"<html><body><h1>{{title}}</h1><p>Lorem ipsum</p>{{#if show}}<p>Extra</p>{{/if}}</body></html>"; 
        var expected = @"<html><body><h1>Title</h1><p>Lorem ipsum</p></body></html>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { 
            { "title", "Title" },
            { "show", "" }
         });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void If_StringConditionWithValue_EvaluatesAsTrue()
    { 
        var template = @"<html><body><h1>{{title}}</h1><p>Lorem ipsum</p>{{#if show}}<p>Extra</p>{{/if}}</body></html>"; 
        var expected = @"<html><body><h1>Title</h1><p>Lorem ipsum</p><p>Extra</p></body></html>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { 
            { "title", "Title" },
            { "show", "a" }
         });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void If_Nested()
    {
        var template = @"<html><body><h1>{{title}}</h1><p>Lorem ipsum</p>{{#if show}}<p>Extra</p>{{#if shownested}}<p>Nested</p>{{/if}}{{/if}}</body></html>"; 
        var expected = @"<html><body><h1>Title</h1><p>Lorem ipsum</p><p>Extra</p><p>Nested</p></body></html>"; 

        var sut = new MailMerge(template);
        var result = sut.Render(new() { 
            { "title", "Title" },
            { "show", true },
            { "shownested", true },
         });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void If_IntCondition_Error()
    { 
        var template = @"<html><body>{{#if show}}<p>Extra</p>{{/if}}</body></html>"; 

        var sut = new MailMerge(template);
        var result = sut.Render(new() { 
            { "show", 1 }
         });

        var error = result.Match(success => new Exception("Unexpected Success"), err => err);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsError, Is.True);
            Assert.That(error, Is.TypeOf<ConditionException>());
        });
    }

    [Test]
    public void IfElse_ConditionFalse_ReturnsElse()
    {
        var template = @"<html><body>{{#if show}}Show{{else}}Else{{/if}}</body></html>";
        var expected = @"<html><body>Else</body></html>";

        var sut = new MailMerge(template);

        var result = sut.Render(new()
        {
            { "show", false },
        });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void IfElse_ConditionTrue_ReturnsIf()
    {
        var template = @"<html><body>{{#if show}}Show{{else}}Else{{/if}}</body></html>";
        var expected = @"<html><body>Show</body></html>";

        var sut = new MailMerge(template);

        var result = sut.Render(new()
        {
            { "show", true },
        });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void IfWithReplaceOnly_ConditionTrue_ReturnsIf()
    {
        var template = @"<html><body>{{#if show}}{{text}}{{/if}}</body></html>";
        var expected = @"<html><body>Lorem ipsum</body></html>";

        var sut = new MailMerge(template);

        var result = sut.Render(new()
        {
            { "show", true },
            { "text", "Lorem ipsum" },
        });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void IfWithReplace_ConditionTrue_ReturnsIf()
    {
        var template = @"<html><body>{{#if some.show}}<p>{{some.text}}</p>{{/if}}</body></html>";
        var expected = @"<html><body><p>Lorem ipsum</p></body></html>";

        var sut = new MailMerge(template);

        var result = sut.Render(new()
        {
            { "some.show", true },
            { "some.text", "Lorem ipsum" },
        });

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }
}
