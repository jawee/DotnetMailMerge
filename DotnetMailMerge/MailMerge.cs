using DotnetMailMerge.Exceptions;
using System.Text.Json;

namespace DotnetMailMerge.Templating;

public class MailMerge
{
    private Dictionary<string, object> _parameters = new();
    private readonly Parser _parser;

    public MailMerge(string template)
    {
        _parser = new(new(template));
    }

    public Result<string, Exception> Render(Dictionary<string, object> parameters)
    {
        _parameters = parameters;
        var parseResult = _parser.Parse();
        if (parseResult.IsError)
        {
            return new Exception("Render failed");
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

            if (result.IsError)
            {
                return result.GetError();
            }

            res += result.GetValue();
        }

        return res;
    }

    private Result<string> HandleLoopBlock(Block block)
    {
        if (block is not LoopBlock b)
        {
            return new UnknownBlockException("Block isn't LoopBlock");
        }

        if (!_parameters.ContainsKey(b.List))
        {
            return new MissingParameterException($"Parameters doesn't contain {b.List}");
        }

        if (_parameters[b.List] is object[] objList)
        {
            var result = "";
            foreach (var obj in objList)
            {
                foreach (var bodyBlock in b.Body)
                {
                    var blockResult = bodyBlock switch
                    {
                        IfBlock => HandleIfBlock(bodyBlock),
                        TextBlock => HandleTextBlock(bodyBlock),
                        ReplaceBlock => HandleReplaceBlockLoop(bodyBlock, obj),
                        _ => throw new NotImplementedException($"unknown block {bodyBlock.GetType()}")
                    };

                    if (blockResult.IsError)
                    {
                        return blockResult.GetError();
                    }

                    result += blockResult.GetValue();
                }
            }
            return result;
        }

        if (_parameters[b.List] is int[] list)
        {
            var result = "";
            foreach (var obj in list)
            {
                foreach (var bodyBlock in b.Body)
                {
                    var blockResult = bodyBlock switch
                    {
                        IfBlock => HandleIfBlock(bodyBlock),
                        TextBlock => HandleTextBlock(bodyBlock),
                        ReplaceBlock => HandleReplaceBlockLoop(bodyBlock, obj),
                        _ => throw new NotImplementedException($"unknown block {bodyBlock.GetType()}")
                    };

                    if (blockResult.IsError)
                    {
                        return blockResult.GetError();
                    }

                    result += blockResult.GetValue();
                }
            }
            return result;
        }

        return new MissingParameterException($"list is null. {_parameters[b.List]}");
    }

    //{ A = 2 }
    private Result<string> HandleReplaceBlockLoop(Block block, object val)
    {
        var res = "";
        if (block is not ReplaceBlock b)
        {
            return new UnknownBlockException("Block isn't ReplaceBlock");
        }

        if (b.Property.Contains("this"))
        {
            if (b.Property is "this")
            {
                res = val.ToString();
            }

            if (b.Property.StartsWith("this."))
            {
                var propName = b.Property.Replace("this.", "");
                var prop = val.GetType().GetProperty(propName);
                if (prop is null)
                {
                    throw new MissingParameterException($"Couldn't find parameter '{propName}' for loop object");
                }

                var newVal = prop.GetValue(val, null);

                if (newVal is null)
                {
                    throw new MissingParameterException($"Couldn't find parameter '{propName}' for loop object");
                }
                res = newVal.ToString();
            }
        }
        else
        {
            if (!_parameters.ContainsKey(b.Property))
            {
                return new MissingParameterException($"Parameters doesn't contain {b.Property}");
            }
            res = _parameters[b.Property].ToString();

            if (res is null)
            {
                return new MissingParameterException($"Parameters doesn't contain {b.Property}");
            }
        }

        return res;
    }

    private Result<string> HandleMdReplaceBlock(Block block)
    {
        if (block is not MdReplaceBlock b)
        {
            return new UnknownBlockException("Block isn't ReplaceBlock");
        }

        var content = b.Content switch
        {
            var a when _parameters.ContainsKey(a) => _parameters[a],
            var a when !_parameters.ContainsKey(a) && b.Content.Contains('.') => GetObjectParameter(a),
            _ => null,
        };

        if (content is null)
        {
            return new MissingParameterException($"Parameters doesn't contain {b.Content}");
        }

        var res = GetHtmlFromMarkdown(content.ToString());

        return res.ToString();
    }

    private string GetHtmlFromMarkdown(string content)
    {
        var lexer = new Markdown.Lexer(content);
        var parser = new Markdown.Parser(lexer);
        var renderer = new Markdown.Renderer(parser);
        return renderer.Render();
    }

    private Result<string> HandleReplaceBlock(Block block)
    {
        if (block is not ReplaceBlock b)
        {
            return new UnknownBlockException("Block isn't ReplaceBlock");
        }

        var res = b.Property switch
        {
            var a when _parameters.ContainsKey(a) => _parameters[a],
            var a when !_parameters.ContainsKey(a) && b.Property.Contains('.') => GetObjectParameter(a),
            _ => new MissingParameterException($"Parameters doesn't contain {b.Property}"),
        };

        if (res is MissingParameterException ex)
        {
            return ex;
        }

        if (res is null)
        {
            return "";
        }

        return res.ToString();
    }

    private object GetObjectParameter(string key)
    {
        var listOfParams = key.Split(".");
        if (_parameters.ContainsKey(listOfParams.First()))
        {
            //TODO: this is chaos
            var param = _parameters[listOfParams.First()];
            //var dict = (Dictionary<string, object>)param;
            if (param is JsonElement superParam)
            {
                var asdfa = superParam.GetRawText();
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(asdfa);
                return dict[listOfParams.Last()];
            }

            if (_parameters[listOfParams.First()] is not Dictionary<string, object> objectDictionary)
            {
                return new MissingParameterException($"Obj {listOfParams.First()} is not a dictionary");
            }

            if (objectDictionary.ContainsKey(listOfParams.Last()))
            {
                return objectDictionary[listOfParams.Last()];
            }
        }
        return new MissingParameterException($"Parameters doesn't contain {key}");
    }

    private Result<string> HandleTextBlock(Block block)
    {
        if (block is not TextBlock b)
        {
            return new UnknownBlockException("Block isn't TextBlock");
        }
        return b.Text;
    }

    private Result<bool> EvaluateCondition(object param)
    {
        bool? res = param switch
        {
            bool => (bool)param,
            string => ((string)param).Length != 0,
            //TODO: shouldn't always be true. Handle properly
            JsonElement => true,
            _ => null,
        };

        if (res is null)
        {
            return new NotImplementedException($"Condition of type {param.GetType()} isn't supported.");
        }

        return res.Value;
    }

    private Result<string> HandleIfBlock(Block block)
    {
        if (block is not IfBlock b)
        {
            return new UnknownBlockException("Block isn't TextBlock");
        }

        var res = b.Condition switch
        {
            var a when _parameters.ContainsKey(a) => _parameters[a],
            var a when !_parameters.ContainsKey(a) && b.Condition.Contains('.') => GetObjectParameter(a),
            _ => null,
        };

        if (res is null)
        {
            return new MissingParameterException($"Parameters doesn't contain {b.Condition}");
        }

        var conditionResult = EvaluateCondition(res);

        if (conditionResult.IsError)
        {
            return new ConditionException(conditionResult.GetError().Message);
        }

        var condition = conditionResult.GetValue();

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

                if (result.IsError)
                {
                    return result.GetError();
                }

                alternative += result.GetValue();
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

            if (result.IsError)
            {
                return result.GetError();
            }

            consRes += result.GetValue();
        }

        return consRes;
    }
}

