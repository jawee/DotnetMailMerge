using System.Text.Json;
using System.Text.Json.Nodes;
using DotnetMailMerge.Exceptions;
using DotnetMailMerge.Templating;

namespace DotnetMailMerge;

public class MailMergeV2
{
    private JsonObject _parameters = new();
    private readonly Parser _parser;

    public MailMergeV2(string template)
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
        if (val.TryGetValue<bool>(out var res))
        {
            return res;
        }
        throw new ConditionException($"Couldn't get bool from {condition.GetPath()}");
    }


    private string HandleLoopBlock(Block block)
    {
        throw new NotImplementedException();
    }

    private string HandleMdReplaceBlock(Block block)
    {
        throw new NotImplementedException();
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
            // var a when !_parameters.ContainsKey(a) && b.Property.Contains('.') => GetObjectParameter(a),
            _ => throw new MissingParameterException($"Parameters doesn't contain {b.Property}"),
        };

        if (res is null)
        {
            return "";
        }

        return res.ToString();

    }

}