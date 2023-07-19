namespace DotnetMailMerge.Exceptions;

public class UnknownBlockException : MailMergeException
{
	public UnknownBlockException(string? message) : base(message)
	{
	}
}

