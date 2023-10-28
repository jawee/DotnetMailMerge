using System.Text.Json;
using System.Text.Json.Nodes;
using DotnetMailMerge.Exceptions;
using DotnetMailMerge.Templating;

namespace DotnetMailMerge;

public class MailMerge
{
    private JsonObject _parameters = new();
    private readonly Parser _parser;

    public MailMerge(string template)
    {
        _parser = new(new(template));
    }

    public string Render(JsonObject parameters)
    {
        _parameters = parameters;
        var parseResult = _parser.Parse();
        if (parseResult.IsError)
        {
            throw new Exception("Render failed");
        }
        var ast = parseResult.GetValue();

        var res = "";
        foreach (var block in ast.Blocks)
        {
            var result = block switch
            {
                IfBlock => HandleIfBlock(block),
                TextBlock => HandleTextBlock(block),
                ReplaceBlock => HandleReplaceBlock(block),
                MdReplaceBlock => HandleMdReplaceBlock(block),
                LoopBlock => HandleLoopBlock(block),
                _ => throw new NotImplementedException($"unknown block {block.GetType()}")
            };

            res += result;
        }

        return res;
    }

    private static string HandleTextBlock(Block block)
    {
        if (block is not TextBlock b)
        {
            throw new UnknownBlockException("Block isn't TextBlock");
        }
        return b.Text;
    }

    private string HandleIfBlock(Block block)
    {
        if (block is not IfBlock b)
        {
            throw new UnknownBlockException("Block isn't IfBlock");
        }

        var res = b.Condition switch
        {
            var a when _parameters.ContainsKey(a) => _parameters[a],
            var a when !_parameters.ContainsKey(a) && b.Condition.Contains('.') => GetObjectParameter(a),
            _ => null,
        };

        if (res is null)
        {
            throw new MissingParameterException($"Parameters doesn't contain {b.Condition}");
        }

        var condition = EvaluateCondition(res);

        if (!condition)
        {
            var alternative = "";
            foreach (var altBlock in b.Alternative)
            {
                var result = altBlock switch
                {
                    IfBlock => HandleIfBlock(altBlock),
                    TextBlock => HandleTextBlock(altBlock),
                    ReplaceBlock => HandleReplaceBlock(altBlock),
                    MdReplaceBlock => HandleMdReplaceBlock(altBlock),
                    _ => throw new NotImplementedException($"unknown block {altBlock.GetType()}")
                };

                alternative += result;
            }

            return alternative;
        }

        var consRes = "";
        foreach (var consB in b.Consequence)
        {
            var result = consB switch
            {
                IfBlock => HandleIfBlock(consB),
                TextBlock => HandleTextBlock(consB),
                ReplaceBlock => HandleReplaceBlock(consB),
                MdReplaceBlock => HandleMdReplaceBlock(consB),
                _ => throw new NotImplementedException($"unknown block {consB.GetType()}")
            };

            consRes += result;
        }

        return consRes;
    }

    private JsonNode? GetObjectParameter(string key)
    {
        var listOfParams = key.Split(".");
        if (_parameters.ContainsKey(listOfParams.First()))
        {
            if (_parameters[listOfParams.First()] is not JsonObject obj)
            {
                throw new MissingParameterException($"Obj {listOfParams.First()} is not a JsonObject");
            }

            if (obj.ContainsKey(listOfParams.Last()))
            {
                var condition = obj[listOfParams.Last()];
                return condition;
            }
        }
        throw new MissingParameterException($"Parameters doesn't contain {key}");
    }

    private static bool EvaluateCondition(JsonNode condition)
    {
        var val = condition.AsValue();
        if (val.TryGetValue<bool>(out var boolRes))
        {
            return boolRes;
        }

        if (val.TryGetValue<string>(out var stringRes))
        {
            return !string.IsNullOrEmpty(stringRes);
        }

        throw new ConditionException($"Couldn't get bool from {condition.GetPath()}");
    }


    private string HandleLoopBlock(Block block)
    {
        if (block is not LoopBlock b)
        {
            throw new UnknownBlockException("Block isn't LoopBlock");
        }

        if (!_parameters.ContainsKey(b.List))
        {
            throw new MissingParameterException($"Parameters doesn't contain {b.List}");
        }

        var result = "";
        if (_parameters[b.List] is JsonArray jsonArray)
        {
            foreach (var node in jsonArray)
            {
                foreach (var bodyBlock in b.Body)
                {
                    var blockResult = bodyBlock switch
                    {
                        IfBlock => HandleIfBlockLoop(bodyBlock, node),
                        TextBlock => HandleTextBlock(bodyBlock),
                        ReplaceBlock => HandleReplaceBlockLoop(bodyBlock, node),
                        _ => throw new NotImplementedException($"unknown block {bodyBlock.GetType()}")
                    };

                    result += blockResult;
                }
            }
            return result;
        }

        throw new MissingParameterException($"list is null. {_parameters[b.List]}");
    }

    private string HandleReplaceBlockLoop(Block block, JsonNode? node)
    {
        if (block is not ReplaceBlock b)
        {
            throw new UnknownBlockException("Block isn't ReplaceBlock");
        }

        if (node is null)
        {
            throw new MissingParameterException("node is null in HandleReplaceBlockLoop");
        }

        if (b.Property is "this")
        {
            return node.ToString();
        }

        // throw new NotImplementedException("Can only handle 'this' in HandleReplaceBlockLoop");
        var res = b.Property switch
        {
            var a when _parameters.ContainsKey(a) => _parameters[a],
            var a when !_parameters.ContainsKey(a) && b.Property.Contains('.') => GetObjectParameter(a, node),
            _ => throw new MissingParameterException($"Parameters doesn't contain {b.Property}"),
        };

        if (res is null)
        {
            return "";
        }

        return res.ToString();


    }

    private JsonNode? GetObjectParameter(string key, JsonNode? node)
    {
        if (node is null)
        {
            throw new MissingParameterException("Missing node in GetObjectParameter for loop");
        }

        if (key.StartsWith("this."))
        {
            key = key.Replace("this.", "");
        }

        var listOfParams = key.Split(".");
        if (node is JsonObject nodeObj && nodeObj.ContainsKey(listOfParams.First()))
        {
            if (listOfParams.Count() is 1)
            {
                return nodeObj[listOfParams.First()];
            }

            if (nodeObj[listOfParams.First()] is not JsonObject obj)
            {
                throw new MissingParameterException($"Obj {listOfParams.First()} is not a JsonObject");
            }

            if (obj.ContainsKey(listOfParams.Last()))
            {
                var condition = obj[listOfParams.Last()];
                return condition;
            }
        }
        throw new MissingParameterException($"Parameters doesn't contain {key}");
    }
    private string HandleIfBlockLoop(Block block, JsonNode? node)
    {
        if (block is not IfBlock b)
        {
            throw new UnknownBlockException("Block isn't IfBlock");
        }

        var res = b.Condition switch
        {
            var a when !_parameters.ContainsKey(a) && b.Condition.StartsWith("this.") => GetObjectParameter(a, node),
            var a when _parameters.ContainsKey(a) => _parameters[a],
            var a when !_parameters.ContainsKey(a) && b.Condition.Contains('.') => GetObjectParameter(a),
            _ => null,
        }; ;

        if (res is null)
        {
            throw new MissingParameterException($"Parameters doesn't contain {b.Condition}");
        }

        var condition = EvaluateCondition(res);

        if (!condition)
        {
            var alternative = "";
            foreach (var altBlock in b.Alternative)
            {
                var result = altBlock switch
                {
                    IfBlock => HandleIfBlock(altBlock),
                    TextBlock => HandleTextBlock(altBlock),
                    ReplaceBlock => HandleReplaceBlock(altBlock),
                    MdReplaceBlock => HandleMdReplaceBlock(altBlock),
                    _ => throw new NotImplementedException($"unknown block {altBlock.GetType()}")
                };

                alternative += result;
            }

            return alternative;
        }

        var consRes = "";
        foreach (var consB in b.Consequence)
        {
            var result = consB switch
            {
                IfBlock => HandleIfBlock(consB),
                TextBlock => HandleTextBlock(consB),
                ReplaceBlock => HandleReplaceBlock(consB),
                MdReplaceBlock => HandleMdReplaceBlock(consB),
                _ => throw new NotImplementedException($"unknown block {consB.GetType()}")
            };

            consRes += result;
        }

        return consRes;
    }

    private string HandleMdReplaceBlock(Block block)
    {
        if (block is not MdReplaceBlock b)
        {
            throw new UnknownBlockException("Block isn't ReplaceBlock");
        }

        var content = b.Content switch
        {
            var a when _parameters.ContainsKey(a) => _parameters[a],
            var a when !_parameters.ContainsKey(a) && b.Content.Contains('.') => GetObjectParameter(a),
            _ => null,
        };

        if (content is null)
        {
            throw new MissingParameterException($"Parameters doesn't contain {b.Content}");
        }

        var res = GetHtmlFromMarkdown(content);

        return res;
    }

    private static string GetHtmlFromMarkdown(JsonNode node)
    {
        var couldParse = node.AsValue().TryGetValue<string>(out var content);
        if (!couldParse || content is null)
        {
            throw new Exception("Couldn't get content for Markdown");
        }
        var lexer = new Markdown.Lexer(content);
        var parser = new Markdown.Parser(lexer);
        var renderer = new Markdown.Renderer(parser);
        return renderer.Render();
    }
    private string HandleReplaceBlock(Block block)
    {
        if (block is not ReplaceBlock b)
        {
            throw new UnknownBlockException("Block isn't ReplaceBlock");
        }

        var res = b.Property switch
        {
            var a when _parameters.ContainsKey(a) => _parameters[a],
            var a when !_parameters.ContainsKey(a) && b.Property.Contains('.') => GetObjectParameter(a),
            _ => throw new MissingParameterException($"Parameters doesn't contain {b.Property}"),
        };

        if (res is null)
        {
            return "";
        }

        return res.ToString();

    }

}