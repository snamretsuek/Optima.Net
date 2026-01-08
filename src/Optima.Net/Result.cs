using System;

namespace Optima.Net.Result
{
    public sealed class Result<T>
    {
        private readonly Result<T, string> _inner;

        private Result(Result<T, string> inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public bool IsSuccess => _inner.IsSuccess;
        public bool IsFailure => _inner.IsFailure;

        public T Value => _inner.Value;

        public string Error => _inner.Error;

        // SUCCESS

        public static Result<T> Ok(T value) =>
            new(Result<T, string>.Ok(value));

        // FAILURE (canonical)

        public static Result<T> Fail(T value, string error) =>
            new(Result<T, string>.Fail(value, error));

        // FAILURE (legacy, backward-compatible)
        [Obsolete("Use Fail(value, error) to preserve the value on failure.")]
        public static Result<T> Fail(string error) =>
            new(Result<T, string>.Fail(error));

        // Conversion to typed error
        // NOTE: Value access is delegated to the inner Result,
        // including any guard rails for legacy failures.
        public Result<T, TError> ToTyped<TError>(Func<string, TError> convertError)
        {
            if (convertError is null)
                throw new ArgumentNullException(nameof(convertError));

            return IsSuccess
                ? Result<T, TError>.Ok(Value)
                : Result<T, TError>.Fail(Value, convertError(Error));
        }
    }
}
