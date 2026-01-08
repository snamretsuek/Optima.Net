using Optima.Net.Result;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Optima.Net.Extensions.Result
{
    public static partial class ResultExtensions
    {
        // ----------------------------
        // BIND
        // ----------------------------

        public static Result<U> Bind<T, U>(
            this Result<T> result,
            Func<T, Result<U>> func) =>
            result.IsFailure
                ? Result<U>.Fail(result.Error)
                : func(result.Value);

        public static async Task<Result<U>> BindAsync<T, U>(
            this Result<T> result,
            Func<T, Task<Result<U>>> func) =>
            result.IsFailure
                ? Result<U>.Fail(result.Error)
                : await func(result.Value).ConfigureAwait(false);

        public static async Task<Result<U>> BindAsync<T, U>(
            this Result<T> result,
            Func<T, CancellationToken, Task<Result<U>>> func,
            CancellationToken cancellationToken = default) =>
            result.IsFailure
                ? Result<U>.Fail(result.Error)
                : await func(result.Value, cancellationToken).ConfigureAwait(false);

        // ----------------------------
        // MAP
        // ----------------------------

        public static Result<U> Map<T, U>(
            this Result<T> result,
            Func<T, U> func) =>
            result.IsFailure
                ? Result<U>.Fail(result.Error)
                : Result<U>.Ok(func(result.Value));

        public static async Task<Result<U>> MapAsync<T, U>(
            this Result<T> result,
            Func<T, Task<U>> func) =>
            result.IsFailure
                ? Result<U>.Fail(result.Error)
                : Result<U>.Ok(await func(result.Value).ConfigureAwait(false));

        public static async Task<Result<U>> MapAsync<T, U>(
            this Result<T> result,
            Func<T, CancellationToken, Task<U>> func,
            CancellationToken cancellationToken = default) =>
            result.IsFailure
                ? Result<U>.Fail(result.Error)
                : Result<U>.Ok(await func(result.Value, cancellationToken).ConfigureAwait(false));

        // ----------------------------
        // TAP
        // ----------------------------

        public static Result<T> Tap<T>(
            this Result<T> result,
            Action<T> action)
        {
            if (result.IsSuccess)
                action(result.Value);

            return result;
        }

        public static async Task<Result<T>> TapAsync<T>(
            this Result<T> result,
            Func<T, Task> action)
        {
            if (result.IsSuccess)
                await action(result.Value).ConfigureAwait(false);

            return result;
        }

        public static async Task<Result<T>> TapAsync<T>(
            this Result<T> result,
            Func<T, CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            if (result.IsSuccess)
                await action(result.Value, cancellationToken).ConfigureAwait(false);

            return result;
        }

        // ----------------------------
        // FAILURE HOOK
        // ----------------------------

        public static Result<T> OnFailure<T>(
            this Result<T> result,
            Action<string> action)
        {
            if (result.IsFailure)
                action(result.Error);

            return result;
        }

        // ----------------------------
        // MATCH
        // ----------------------------

        public static TResult Match<T, TResult>(
            this Result<T> result,
            Func<T, TResult> onSuccess,
            Func<string, TResult> onFailure) =>
            result.IsSuccess
                ? onSuccess(result.Value)
                : onFailure(result.Error);

        public static async Task<TResult> MatchAsync<T, TResult>(
            this Result<T> result,
            Func<T, Task<TResult>> onSuccess,
            Func<string, Task<TResult>> onFailure) =>
            result.IsSuccess
                ? await onSuccess(result.Value).ConfigureAwait(false)
                : await onFailure(result.Error).ConfigureAwait(false);

        public static async Task<TResult> MatchAsync<T, TResult>(
            this Result<T> result,
            Func<T, CancellationToken, Task<TResult>> onSuccess,
            Func<string, CancellationToken, Task<TResult>> onFailure,
            CancellationToken cancellationToken = default) =>
            result.IsSuccess
                ? await onSuccess(result.Value, cancellationToken).ConfigureAwait(false)
                : await onFailure(result.Error, cancellationToken).ConfigureAwait(false);
    }
}
