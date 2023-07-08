namespace DotnetMailMerge.Exceptions;

public class MissingParameterException : MailMergeException
{
	public MissingParameterException(string? message) : base(message)
	{
	}
}

