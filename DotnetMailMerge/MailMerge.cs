namespace DotnetMailMerge;

public class MailMerge
{
    private string _template;
    private readonly Dictionary<string, object> _parameters;
    public MailMerge(string template, Dictionary<string, object> parameters)
    {
        _template = template;
        _parameters = parameters;
    }

    public Result<string, Exception> Render() 
    {
        foreach (var (key, value) in _parameters)
        {
            var newKey = "{{" + key + "}}";
            _template = _template.Replace(newKey, value as string);
        }

        return _template;
    }

}

