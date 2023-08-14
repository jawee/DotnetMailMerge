using DotnetMailMerge.Exceptions;

namespace DotnetMailMerge.Markdown;

public class Renderer
{
	private readonly Parser _parser;
	private IBlock? _currentBlock;
	private int _readPos;
	private readonly List<IBlock> _blocks;
	public Renderer(Parser parser)
	{
		_parser = parser;
		var parserResult = _parser.Parse();

		var ast = parserResult.GetValue();

		_blocks = ast.Blocks;
		_readPos = 0;
		NextBlock();
	}

	private void NextBlock()
	{
		_currentBlock = _readPos < _blocks.Count ? _blocks[_readPos] : null;
		_readPos++;
    }
	public string Render()
	{
		var str = "";
		while (_currentBlock is not null)
		{ 
			str += RenderBlock(_currentBlock);
			NextBlock();
        }

		return str;
    }

    private string RenderBlock(IBlock block)
	{
		var str = block switch
		{
			HeadingBlock a => RenderHeadingBlock(a),
			ParagraphBlock a => RenderParagraphBlock(a),
			ItemBlock => RenderItemBlock(),
			_  => throw new UnknownBlockException("Unknown block for Renderer"),
		};

		return str;
    }

	private string RenderItemBlock()
	{
		var str = "<ul>";
		while(_currentBlock is ItemBlock a)
		{
            str += $"<li>{a.Text}</li>";
			NextBlock();
        }
        str += "</ul>";

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

