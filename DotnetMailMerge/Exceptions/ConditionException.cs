namespace DotnetMailMerge.Exceptions;

public class ConditionException : MailMergeException
{
	public ConditionException(string? message) : base(message)
	{
	}
}

