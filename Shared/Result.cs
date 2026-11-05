namespace Shared;
public class Result
{
    protected Result(bool isSuccess, Error? error, SuccessType? successType)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error is null)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
        SuccessType = successType;
    }

    public bool IsSuccess { get; }
    public Error? Error { get; }
    public bool IsFailure => !IsSuccess;
    public SuccessType? SuccessType { get; }
    public static Result Success() => new(true, null, Shared.SuccessType.Ok);

    public static Result Failure(Error error) => new(false, error, Shared.SuccessType.No);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;
    private Result(bool isSuccess, TValue? value, Error? error = null, SuccessType? successType = null)
        : base(isSuccess, error, successType) => _value = value;

    public TValue Value => IsSuccess ? _value! :
        throw new InvalidOperationException("Cannot access the value of a failed result.");
    public static Result<TValue> Success(TValue value) => new(true, value, null, Shared.SuccessType.Ok);
    public new static Result<TValue> Failure(Error error) => new(false, default, error, Shared.SuccessType.No);

    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        if (IsSuccess)
            return onSuccess(_value!);
        else
            return onFailure(Error!);
    }

    public static implicit operator Result<TValue>(TValue value) => Success(value);

}