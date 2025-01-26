namespace DotnetMailMerge.Exceptions;

public class TemplatingException : MailMergeException
{
    public TemplatingException(string message) : base(message)
    {
    }
}
