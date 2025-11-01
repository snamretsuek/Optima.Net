using Optima.Net.Exceptions;

namespace Optima.Net
{
    public sealed class Result<T, TError>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public T Value { get; }
        public TError Error { get; }

        private Result(T value, TError error, bool isSuccess)
        {
            if (isSuccess && value is null)
                throw NullValueException.ForResultType<T>();
            //new NullValueException(nameof(value),
            //"Cannot create a successful Result with a null value.");

            if (!isSuccess && error is null)
                throw NullValueException.ForResultType<TError>();
            //new NullValueException(nameof(error),
            //    "Cannot create a failed Result with a null error.");

            Value = value;
            Error = error;
            IsSuccess = isSuccess;
        }

        public static Result<T, TError> Ok(T value) =>
            new(value, default!, true);

        public static Result<T, TError> Fail(TError error) =>
            new(default!, error, false);
    }
}

