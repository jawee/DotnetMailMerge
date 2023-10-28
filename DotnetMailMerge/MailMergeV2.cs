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

    private string HandleIfBlock(Block block)
    {
        throw new NotImplementedException();
    }
}