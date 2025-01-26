namespace DotnetMailMerge.Exceptions;

public class MailMergeFailedException : MailMergeException
{
    public MailMergeFailedException(string message) : base(message)
    {
    }
}
