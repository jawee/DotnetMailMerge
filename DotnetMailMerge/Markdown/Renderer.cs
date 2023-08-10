using DotnetMailMerge.Exceptions;

namespace DotnetMailMerge.Markdown;

public class Renderer
{
	private readonly Parser _parser;
	public Renderer(Parser parser)
	{
		_parser = parser;
	}

	public string Render()
	{
		var parserResult = _parser.Parse();

		var ast = parserResult.GetValue();

		var str = "";
		foreach (var block in ast.Blocks)
		{
			str += RenderBlock(block);
			str += "\n";
        }

        str = str[..^"\n".Length];


		return str;
    }

    private static string RenderBlock(IBlock block)
	{
		var str = block switch
		{
			HeadingBlock a => RenderHeadingBlock(a),
			ParagraphBlock a => RenderParagraphBlock(a),
			_  => throw new UnknownBlockException("Unknown block for Renderer"),
		};

		return str;
    }

    private static string RenderParagraphBlock(ParagraphBlock a)
    {
		return $"<p>{a.Text}</p>";
    }

    private static string RenderHeadingBlock(HeadingBlock block)
	{
		return $"<h{block.Level}>{block.Text}</h{block.Level}>";
    }
}

