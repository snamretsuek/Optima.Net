using Optima.Net.Exceptions;

namespace Optima.Net.Result
{
    public sealed class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T Value { get; }
        public string Error { get; }

        private Result(T value, bool isSuccess, string error)
        {
            if (isSuccess && value is null)
                throw new NullValueException("Cannot create a successful Result with a null value.");

            Value = value;
            IsSuccess = isSuccess;
            Error = error;
        }

        // Factory methods
        public static Result<T> Ok(T value) => new Result<T>(value, true, string.Empty);
        public static Result<T> Fail(string error) => new Result<T>(default!, false, error);
    }
}
