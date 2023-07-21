﻿using DotnetMailMerge.Exceptions;

namespace DotnetMailMerge;

public class MailMerge
{
    private readonly Dictionary<string, object> _parameters;
    private readonly Parser _parser;
    public MailMerge(string template, Dictionary<string, object> parameters)
    {
        _parameters = parameters;
        var lexer = new Lexer(template);
        _parser = new Parser(lexer);
    }

    public Result<string, Exception> Render() 
    {
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

    private Result<string> HandleReplaceBlock(Block block)
    {
        var b = block as ReplaceBlock;
        if (b is null)
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
        var b = block as TextBlock;

        if (b is null)
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
        var b = block as IfBlock;

        if (b is null)
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

