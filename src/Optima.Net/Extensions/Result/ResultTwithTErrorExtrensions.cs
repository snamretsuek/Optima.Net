using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optima.Net.Extensions.Result
{
    public static partial class ResultExtensions
    {
        public static Result<U, TError> Map<T, U, TError>(
            this Result<T, TError> result,
            Func<T, U> func) =>
            result.IsFailure
                ? Result<U, TError>.Fail(result.Error)
                : Result<U, TError>.Ok(func(result.Value));

        public static Result<U, TError> Bind<T, U, TError>(
            this Result<T, TError> result,
            Func<T, Result<U, TError>> func) =>
            result.IsFailure
                ? Result<U, TError>.Fail(result.Error)
                : func(result.Value);

        public static TResult Match<T, TError, TResult>(
            this Result<T, TError> result,
            Func<T, TResult> onSuccess,
            Func<TError, TResult> onError) =>
            result.IsSuccess ? onSuccess(result.Value) : onError(result.Error);

        /// <summary>
        /// Ensures a successful result meets a condition, otherwise returns a failure using the supplied error factory.
        /// </summary>
        public static Result<T, TError> Ensure<T, TError>(
            this Result<T, TError> result,
            Func<T, bool> predicate,
            Func<TError> errorFactory)
        {
            if (result.IsFailure)
                return Result<T, TError>.Fail(result.Error);

            return predicate(result.Value)
                ? result
                : Result<T, TError>.Fail(errorFactory());
        }

        /// <summary>
        /// Asynchronously ensures a successful result meets a condition, otherwise returns a failure using the supplied error factory.
        /// </summary>
        public static async Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Result<T, TError> result,
            Func<T, Task<bool>> predicate,
            Func<TError> errorFactory)
        {
            if (result.IsFailure)
                return Result<T, TError>.Fail(result.Error);

            return await predicate(result.Value).ConfigureAwait(false)
                ? result
                : Result<T, TError>.Fail(errorFactory());
        }

        /// <summary>
        /// Awaits a Result task then ensures it meets a condition, otherwise returns a failure using the supplied error factory.
        /// </summary>
        public static async Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Task<Result<T, TError>> resultTask,
            Func<T, Task<bool>> predicate,
            Func<TError> errorFactory)
        {
            var result = await resultTask.ConfigureAwait(false);

            if (result.IsFailure)
                return Result<T, TError>.Fail(result.Error);

            return await predicate(result.Value).ConfigureAwait(false)
                ? result
                : Result<T, TError>.Fail(errorFactory());
        }

        /// <summary>
        /// Awaits a Result task then ensures it meets a condition, otherwise returns a failure using the supplied error factory.
        /// </summary>
        public static async Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Task<Result<T, TError>> resultTask,
            Func<T, bool> predicate,
            Func<TError> errorFactory)
        {
            var result = await resultTask.ConfigureAwait(false);

            if (result.IsFailure)
                return Result<T, TError>.Fail(result.Error);

            return predicate(result.Value)
                ? result
                : Result<T, TError>.Fail(errorFactory());
        }

        /// <summary>
        /// Asynchronously ensures a successful result meets a condition, with cancellation support.
        /// </summary>
        public static async Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Result<T, TError> result,
            Func<T, CancellationToken, Task<bool>> predicate,
            Func<TError> errorFactory,
            CancellationToken cancellationToken = default)
        {
            if (result.IsFailure)
                return Result<T, TError>.Fail(result.Error);

            cancellationToken.ThrowIfCancellationRequested();

            return await predicate(result.Value, cancellationToken).ConfigureAwait(false)
                ? result
                : Result<T, TError>.Fail(errorFactory());
        }

        /// <summary>
        /// Awaits a Result task then ensures it meets a condition, with cancellation support.
        /// </summary>
        public static async Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Task<Result<T, TError>> resultTask,
            Func<T, CancellationToken, Task<bool>> predicate,
            Func<TError> errorFactory,
            CancellationToken cancellationToken = default)
        {
            var result = await resultTask.ConfigureAwait(false);

            if (result.IsFailure)
                return Result<T, TError>.Fail(result.Error);

            cancellationToken.ThrowIfCancellationRequested();

            return await predicate(result.Value, cancellationToken).ConfigureAwait(false)
                ? result
                : Result<T, TError>.Fail(errorFactory());
        }
    }

}
