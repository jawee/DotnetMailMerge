using NUnit.Framework;
using DotnetMailMerge.Exceptions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DotnetMailMerge.Tests;

public class MailMergeTestsV2
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

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "title", "Title" } });

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void StringReplacement_MultipleOccurrence()
    {
        var template = @"<html><body><h1>{{title}}{{title}}{{title}}</h1></body></html>";
        var expected = @"<html><body><h1>TitleTitleTitle</h1></body></html>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "title", "Title" } });

        Assert.That(result, Is.EqualTo(expected));
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

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void StringReplacement_NotAllReplaced_ThrowsMissingParameterException()
    {
        var template = @"<html><body><h1>{{title}}</h1><p>{{body}}</p></body></html>";

        var sut = new MailMerge(template);

        Assert.Throws<MissingParameterException>(() => sut.Render(new() { { "title", "Title" } }));
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

        Assert.That(result, Is.EqualTo(expected));
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

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void If_ObjectProperty_ConditionTrue()
    {
        var template = @"<html><body><h1>{{title}}</h1><p>Lorem ipsum</p>{{#if someobj.show}}<p>Extra</p>{{/if}}</body></html>";
        var expected = @"<html><body><h1>Title</h1><p>Lorem ipsum</p><p>Extra</p></body></html>";

        var showJson = @"
            {
            ""show"": true
            }";

        var showObj = JsonSerializer.Deserialize<JsonNode>(showJson);
        var sut = new MailMerge(template);
        var result = sut.Render(new() {
            { "title", "Title" },
            { "someobj", showObj },
         });

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void If_ObjectProperty_ConditionFalse()
    {
        var template = @"<html><body><h1>{{title}}</h1><p>Lorem ipsum</p>{{#if someobj.show}}<p>Extra</p>{{/if}}</body></html>"; 
        var expected = @"<html><body><h1>Title</h1><p>Lorem ipsum</p></body></html>";

        var showJson = @"
            {
            ""show"": false
            }";

        var showObj = JsonSerializer.Deserialize<JsonNode>(showJson);
        var sut = new MailMerge(template);
        var result = sut.Render(new() {
            { "title", "Title" },
            { "someobj", showObj },
         });

        Assert.That(result, Is.EqualTo(expected));
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

        Assert.That(result, Is.EqualTo(expected));
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

        Assert.That(result, Is.EqualTo(expected));
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

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void If_IntCondition_Error()
    { 
        var template = @"<html><body>{{#if show}}<p>Extra</p>{{/if}}</body></html>"; 

        var sut = new MailMerge(template);
        Assert.Throws<ConditionException>(() => sut.Render(new() { 
            { "show", 1 }
         }));
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

        Assert.That(result, Is.EqualTo(expected));
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

        Assert.That(result, Is.EqualTo(expected));
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

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void IfWithReplace_ConditionTrue_ReturnsIf()
    {
        var template = "<html><body>{{#if some.show}}<p style=\"Margin: 0;font-size=13px;\">{{some.text}}</p>{{/if}}</body></html>";
        var expected = "<html><body><p style=\"Margin: 0;font-size=13px;\">Lorem ipsum</p></body></html>";

        var sut = new MailMerge(template);

        var result = sut.Render(new()
        {
            { "some.show", true },
            { "some.text", "Lorem ipsum" },
        });

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void IfWithReplace_ConditionTrueWithReplaceStartsWithE_ReturnsIf()
    {
        var template = "<html><body>{{#if some.show}}{{esome.text}}{{/if}}</body></html>";
        var expected = "<html><body>Lorem ipsum</body></html>";

        var sut = new MailMerge(template);

        var result = sut.Render(new()
        {
            { "some.show", true },
            { "esome.text", "Lorem ipsum" },
        });

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void MarkdownParagraphReplace()
    {
        var template = @"<body><div>{{{ paragraph }}}</div></body>";
        var expected = @"<body><div><p>Lorem ipsum</p></div></body>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "paragraph", "Lorem ipsum"} });

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void MarkdownItemReplace()
    {
        var template = @"<body><div>{{{ items }}}</div></body>";
        var expected = @"<body><div><ul><li>A</li><li>B</li></ul></div></body>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "items", "* A\n* B"} });

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void MarkdownReplaceHeadingAndParagraph()
    {
        var template = "<body><div>{{{ content }}}</div></body>";
        var expected = "<body><div><h1>Heading</h1><p>Lorem ipsum</p></div></body>";

        var sut = new MailMerge(template);
        var result = sut.Render(new() { { "content", "# Heading\nLorem ipsum"} });

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ReplaceFromObjectProperty()
    {
        var template = "{{ someobj.someprop }}";
        var expected = "test";

        var sut = new MailMerge(template);

        var json = @"
        {
            ""someobj"": {
                ""someprop"": ""test""
            }

        }";
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(json);
        var result = sut.Render(jsonObj);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ReplaceMdFromObjectProperty()
    {
        var template = "{{{ someobj.someprop }}}";
        var expected = "<p>test</p>";

        var sut = new MailMerge(template);

        var json = @"
        {
            ""someobj"": {
                ""someprop"": ""test""
            }

        }";
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(json);
        var result = sut.Render(jsonObj);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Loop_ListOfInts_WithoutAccessingValue()
    {
        var template = "{{#each items}}<p>Item</p>{{/each}}";
        var expected = "<p>Item</p><p>Item</p>";

        var sut = new MailMerge(template);

        var json = @"{
            ""items"": [1,2]
        }";
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(json);

        var result = sut.Render(jsonObj);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Loop_ListOfInts_AccessingValue()
    {
        var template = "{{#each items}}<p>{{this}}</p>{{/each}}";
        var expected = "<p>1</p><p>2</p>";

        var sut = new MailMerge(template);

        var json = @"{
            ""items"": [1,2]
        }";
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(json);
        var result = sut.Render(jsonObj);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Loop_ListOfObject_WithoutAccessingProperties()
    {
        var template = "{{#each items}}<p>Item</p>{{/each}}";
        var expected = "<p>Item</p><p>Item</p>";

        var sut = new MailMerge(template);

        var json = @"{
            ""items"": [{ ""A"": 1 }, { ""A"": 2 }]
        }";
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(json);
        var result = sut.Render(jsonObj);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Loop_ListOfObject_AccessingProperties()
    {
        var template = "{{#each items}}<p>{{this.A}}</p>{{/each}}";
        var expected = "<p>1</p><p>2</p>";

        var sut = new MailMerge(template);

        var json = @"{
            ""items"": [{ ""A"": 1 }, { ""A"": 2 }]
        }";
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(json);
        var result = sut.Render(jsonObj);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Loop_ListOfObject_WithRegularReplace()
    {
        var template = "{{#each items}}<p>{{text}}</p>{{/each}}";
        var expected = "<p>text</p><p>text</p>";

        var sut = new MailMerge(template);

        var json = @"{
            ""text"": ""text"",
            ""items"": [{ ""A"": 1 }, { ""A"": 2 }]
        }";
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(json);
        var result = sut.Render(jsonObj);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Loop_ListOfObject_WithIf()
    {
        var template = "{{#each items}}{{#if this.show}}<p>text</p>{{/if}}{{/each}}";
        var expected = "<p>text</p>";

        var sut = new MailMerge(template);

        var json = @"{
            ""items"": [{ ""show"": true, ""A"": 1 }, { ""show"": false, ""A"": 2 }]
        }";
        var jsonObj = JsonSerializer.Deserialize<JsonObject>(json);
        var result = sut.Render(jsonObj);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void LoopWithIf_JsonElement_ObjectProperty_ConditionFalse()
    {
        var template = "{{#each items}}{{#if this.show}}<p>text</p>{{/if}}{{/each}}";
        var expected = "<p>text</p>";

        var sut = new MailMerge(template);
        var showJson = @"
        {
            ""items"": [ { ""show"": true } ]
        }";
        var showObj = JsonSerializer.Deserialize<JsonObject>(showJson);
        var result = sut.Render(showObj);

        Assert.That(result, Is.EqualTo(expected));
    }
}
