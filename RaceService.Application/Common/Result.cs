namespace RaceService.Application.Common;

/// <summary>
/// The outcome of an operation: either success, or a described failure.
/// </summary>
/// <remarks>
/// Expected failures — a slug already taken, an event that does not exist — are returned rather
/// than thrown. Exceptions are for the genuinely exceptional; using them for a 404 makes the
/// happy path slower, the stack traces noisier, and the control flow invisible in the signature.
/// </remarks>
public class Result
{
    /// <summary>Initialises a result.</summary>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A successful result cannot carry an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("A failed result must carry an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    /// <summary>Creates a success.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failure.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Creates a success carrying a value.</summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>Creates a failure of a value-carrying result.</summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>An outcome that carries a value on success.</summary>
/// <typeparam name="TValue">Type of the value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>Initialises a result.</summary>
    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
        => _value = value;

    /// <summary>The value. Throws if the result is a failure.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    /// <summary>Wraps a value as a success.</summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
