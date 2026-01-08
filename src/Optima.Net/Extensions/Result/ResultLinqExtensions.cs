using Optima.Net.Result;
using System;
using System.Collections.Generic;

namespace Optima.Net.Extensions.Result.LINQ
{
    public static class ResultLinqExtensions
    {
        /// <summary>
        /// LINQ Select support for Result&lt;T&gt;
        /// </summary>
        public static Result<U> Select<T, U>(
            this Result<T> result,
            Func<T, U> selector) =>
            result.IsFailure
                ? Result<U>.Fail(default!, result.Error)
                : Result<U>.Ok(selector(result.Value));

        /// <summary>
        /// LINQ SelectMany support for Result&lt;T&gt;
        /// </summary>
        public static Result<V> SelectMany<T, U, V>(
            this Result<T> result,
            Func<T, Result<U>> bind,
            Func<T, U, V> project)
        {
            if (result.IsFailure)
                return Result<V>.Fail(default!, result.Error);

            var bound = bind(result.Value);

            return bound.IsFailure
                ? Result<V>.Fail(default!, bound.Error)
                : Result<V>.Ok(project(result.Value, bound.Value));
        }

        /// <summary>
        /// LINQ Where support for IEnumerable&lt;Result&lt;T&gt;&gt;
        /// Failures are preserved.
        /// Predicate failures turn successes into failures without changing the value.
        /// </summary>
        public static IEnumerable<Result<T>> Where<T>(
            this IEnumerable<Result<T>> source,
            Func<T, bool> predicate)
        {
            foreach (var r in source)
            {
                if (r.IsFailure)
                {
                    yield return r;
                }
                else if (predicate(r.Value))
                {
                    yield return r;
                }
                else
                {
                    yield return Result<T>.Fail(r.Value, "Filtered out");
                }
            }
        }

        /// <summary>
        /// LINQ Select support for Result&lt;T, TError&gt;
        /// </summary>
        public static Result<U, TError> Select<T, U, TError>(
            this Result<T, TError> result,
            Func<T, U> selector) =>
            result.IsFailure
                ? Result<U, TError>.Fail(default!, result.Error)
                : Result<U, TError>.Ok(selector(result.Value));

        /// <summary>
        /// LINQ SelectMany support for Result&lt;T, TError&gt;
        /// </summary>
        public static Result<V, TError> SelectMany<T, U, V, TError>(
            this Result<T, TError> result,
            Func<T, Result<U, TError>> bind,
            Func<T, U, V> project)
        {
            if (result.IsFailure)
                return Result<V, TError>.Fail(default!, result.Error);

            var bound = bind(result.Value);

            return bound.IsFailure
                ? Result<V, TError>.Fail(default!, bound.Error)
                : Result<V, TError>.Ok(project(result.Value, bound.Value));
        }
    }
}
