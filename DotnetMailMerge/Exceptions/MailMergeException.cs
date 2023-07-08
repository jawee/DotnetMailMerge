namespace DotnetMailMerge.Exceptions;

public abstract class MailMergeException : Exception
{
    protected MailMergeException(string? message) : base(message)
    { 
    }
}

