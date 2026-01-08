using Optima.Net.Exceptions;
using System;

namespace Optima.Net.Result
{
    public sealed class Result<T, TError>
    {
        private readonly T _value;
        private readonly TError _error;
        private readonly bool _hasValue;

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public T Value =>
            _hasValue
                ? _value
                : throw new InvalidOperationException(
                    "Value is not available for this failure. " +
                    "This Result was created using Fail(error). " +
                    "Use Fail(value, error) to preserve the value.");

        public TError Error =>
            IsFailure
                ? _error
                : throw new InvalidOperationException(
                    "Error accessed on a successful Result.");

        private Result(
            T value,
            TError error,
            bool isSuccess,
            bool hasValue)
        {
            if (isSuccess && value is null)
                throw NullValueException.ForResultType<T>();
                //new NullValueException(nameof(value),
                //"Cannot create a successful Result with a null value.");

            if (!isSuccess && error is null)
                throw NullValueException.ForResultType<TError>();
                //new NullValueException(nameof(error),
                //    "Cannot create a failed Result with a null error.");

            _value = value;
            _error = error;
            _hasValue = hasValue;
            IsSuccess = isSuccess;
        }

        // SUCCESS

        public static Result<T, TError> Ok(T value) =>
            new Result<T, TError>(
                value,
                default!,
                isSuccess: true,
                hasValue: true);

        // FAILURE (new canonical form)

        public static Result<T, TError> Fail(T value, TError error) =>
            new Result<T, TError>(
                value,
                error,
                isSuccess: false,
                hasValue: true);

        // FAILURE (backward-compatible legacy form)
        [Obsolete("Use Fail(value, error) to preserve the value on failure.")]
        public static Result<T, TError> Fail(TError error) =>
            new Result<T, TError>(
                default!,
                error,
                isSuccess: false,
                hasValue: false);
    }
}
