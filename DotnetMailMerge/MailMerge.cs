using DotnetMailMerge.Exceptions;

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
                _ => throw new NotImplementedException("unknown block")
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
                        _ => throw new NotImplementedException("unknown block")
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
                        _ => throw new NotImplementedException("unknown block")
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

    private Result<string> HandleReplaceBlockLoop(Block block, object val)
    {
        if (block is not ReplaceBlock b)
        {
            return new UnknownBlockException("Block isn't ReplaceBlock");
        }

        if (b.Property is not "this")
        {
            return new NotImplementedException("ReplaceBlock in loop can only handle this.");
        }

        var res = val;

        return res.ToString();
    }

    private Result<string> HandleMdReplaceBlock(Block block)
    {
        if (block is not MdReplaceBlock b)
        { 
            return new UnknownBlockException("Block isn't MdReplaceBlock");
        }

        if (!_parameters.ContainsKey(b.Content))
        {
            return new MissingParameterException($"Parameters doesn't contain {b.Content}");
        }

        var content = _parameters[b.Content];

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

        if (!_parameters.ContainsKey(b.Property))
        {
            return new MissingParameterException($"Parameters doesn't contain {b.Property}");
        }
        var res = _parameters[b.Property];

        if (res is null)
        { 
            return new MissingParameterException($"Parameters doesn't contain {b.Property}");
        }

        return res.ToString();
    }

    private Result<string> HandleTextBlock(Block block)
    {
        if (block is not TextBlock b)
        {
            return new UnknownBlockException("Block isn't TextBlock");
        }
        return b.Text;
    }

    private Result<bool> EvaluateCondition(string conditionKey)
    {
        var param = _parameters[conditionKey];

        if (param is null)
        {
            return new MissingParameterException(nameof(conditionKey));
        }

        bool? res = param switch
        {
            bool => (bool)param,
            string => ((string)param).Length != 0,
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

        if (!_parameters.ContainsKey(b.Condition))
        {
            return new MissingParameterException($"Parameters doesn't contain {b.Condition}");
        }

        var conditionResult = EvaluateCondition(b.Condition);

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
                    _ => throw new NotImplementedException("unknown block")
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
                _ => throw new NotImplementedException("unknown block")
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

