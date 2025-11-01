namespace Optima.Net.Result
{
    public sealed class Result<T>
    {
        private readonly Result<T, string> _inner;

        private Result(Result<T, string> inner) => _inner = inner;

        public bool IsSuccess => _inner.IsSuccess;
        public bool IsFailure => _inner.IsFailure;
        public T Value => _inner.Value;
        public string Error => _inner.Error;

        public static Result<T> Ok(T value) =>
            new(Result<T, string>.Ok(value));

        public static Result<T> Fail(string error) =>
            new(Result<T, string>.Fail(error));

        public Result<T, TError> ToTyped<TError>(Func<string, TError> convertError) =>
            _inner.IsSuccess
                ? Result<T, TError>.Ok(_inner.Value)
                : Result<T, TError>.Fail(convertError(_inner.Error));

        public Result<T, string> Inner() => _inner;
    }
}
