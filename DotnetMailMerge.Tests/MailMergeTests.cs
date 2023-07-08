using NUnit.Framework;
using DotnetMailMerge.Exceptions;

namespace DotnetMailMerge.Tests;

public class MailMergeTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void StringReplacement_SingleOccurrence()
    {
        var template = @"<html><body><h1>{{title}}</h1></body></html>";
        var expected = @"<html><body><h1>Title</h1></body></html>";

        var sut = new MailMerge(template, new() { { "title", "Title" } });
        var result = sut.Render();

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }

    [Test]
    public void StringReplacement_MultipleOccurrence()
    {
        var template = @"<html><body><h1>{{title}}{{title}}{{title}}</h1></body></html>";
        var expected = @"<html><body><h1>TitleTitleTitle</h1></body></html>";

        var sut = new MailMerge(template, new() { { "title", "Title" } });
        var result = sut.Render();

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }


    [Test]
    public void StringReplacement_MultipleParameters()
    {
        var template = @"<html><body><h1>{{title}}</h1><p>{{body}}</p></body></html>";
        var expected = @"<html><body><h1>Title</h1><p>Body</p></body></html>";

        var sut = new MailMerge(template, new() { 
            { "title", "Title" },
            { "body", "Body" }
         });
        var result = sut.Render();

        Assert.That(result.Match(success => success, _ => ""), Is.EqualTo(expected));
    }
    [Test]
    public void StringReplacement_NotAllReplaced_ReturnsError()
    {
        var template = @"<html><body><h1>{{title}}</h1><p>{{body}}</p></body></html>";

        var sut = new MailMerge(template, new() { { "title", "Title" } });
        var result = sut.Render();

        var error = result.Match(success => new Exception("Unexpected Success"), err => err);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsError, Is.True);
            Assert.That(error, Is.TypeOf<MissingParameterException>());
        });
    }
}
