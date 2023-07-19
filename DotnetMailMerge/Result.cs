namespace DotnetMailMerge;

public readonly struct Result<TValue>
{
	private readonly TValue? _value;
	private readonly Exception? _error;

    public bool IsError { get; }
    public bool IsSuccess => !IsError;
	private Result(TValue value)
	{
		IsError = false;
		_value = value;
		_error = default;
    }

	private Result(Exception error)
	{
		IsError = true;
		_value = default;
		_error = error;
    }

	public TValue GetValue() 
    {
		return _value!;
	}

	public Exception GetError()
	{
		return _error!;
    }

	public static implicit operator Result<TValue>(TValue value) => new(value);
	public static implicit operator Result<TValue>(Exception error) => new(error);

	public TResult Match<TResult>(
        Func<TValue, TResult> success, 
        Func<Exception, TResult> failure) => 
        !IsError ? success(_value!) : failure(_error!);
}

public readonly struct Result<TValue, TError>
{
	private readonly TValue? _value;
	private readonly TError? _error;

    public bool IsError { get; }
    public bool IsSuccess => !IsError;

	private Result(TValue value)
	{
		IsError = false;
		_value = value;
		_error = default;
    }

	private Result(TError error)
	{
		IsError = true;
		_value = default;
		_error = error;
    }

	public TValue GetValue() 
    {
		return _value!;
	}

	public TError GetError()
	{
		return _error!;
    }

	public static implicit operator Result<TValue, TError>(TValue value) => new(value);
	public static implicit operator Result<TValue, TError>(TError error) => new(error);

	public TResult Match<TResult>(
        Func<TValue, TResult> success, 
        Func<TError, TResult> failure) => 
        !IsError ? success(_value!) : failure(_error!);
}

