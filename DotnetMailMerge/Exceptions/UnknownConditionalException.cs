namespace DotnetMailMerge.Exceptions;

public class UnknownConditionalException : MailMergeException
{
	public UnknownConditionalException(string? message) : base(message)
	{
	}
}

