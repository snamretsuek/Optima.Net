using Optima.Net.Result;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Optima.Net.Extensions.Result
{
    public static class ResultCollectionExtension
    {
        /// <summary>
        /// Aggregates many Result&lt;T&gt; into a Result&lt;IEnumerable&lt;T&gt;&gt;.
        /// If any element failed, the aggregate fails.
        /// Values are preserved where available.
        /// </summary>
        public static Result<IEnumerable<T>> Sequence<T>(
            this IEnumerable<Result<T>> results)
        {
            var list = results.ToList();

            var errors = list
                .Where(r => r.IsFailure)
                .Select(r => r.Error)
                .ToList();

            // This intentionally accesses Value.
            // Legacy failures will throw here, by design.
            var values = list.Select(r => r.Value).ToList();

            return errors.Any()
                ? Result<IEnumerable<T>>.Fail(values, string.Join("; ", errors))
                : Result<IEnumerable<T>>.Ok(values);
        }

        /// <summary>
        /// Async variant of Sequence.
        /// </summary>
        public static async Task<Result<IEnumerable<T>>> SequenceAsync<T>(
            this IEnumerable<Task<Result<T>>> tasks,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var wrapped = tasks.Select(async t =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await t.ConfigureAwait(false);
            });

            var results = await Task.WhenAll(wrapped).ConfigureAwait(false);
            return results.Sequence();
        }

        /// <summary>
        /// Filters a collection of Result&lt;T&gt; by a predicate.
        /// Failures are preserved.
        /// Predicate failures convert successes into failures without changing the value.
        /// </summary>
        public static IEnumerable<Result<T>> Where<T>(
            this IEnumerable<Result<T>> results,
            Func<T, bool> predicate)
        {
            foreach (var r in results)
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
        /// Flattens Result&lt;Result&lt;T&gt;&gt; into Result&lt;T&gt;.
        /// Outer failure is propagated.
        /// </summary>
        public static Result<T> Flatten<T>(
            this Result<Result<T>> result) =>
            result.IsFailure
                ? Result<T>.Fail(result.Value.Value, result.Error)
                : result.Value;

        /// <summary>
        /// Asynchronously filters a collection of Result&lt;T&gt; using an async predicate.
        /// - Existing failures are preserved.
        /// - Successful values failing the predicate become failures ("Filtered out").
        /// - Values are preserved.
        /// </summary>
        public static async Task<IEnumerable<Result<T>>> WhereAsync<T>(
            this IEnumerable<Result<T>> source,
            Func<T, CancellationToken, Task<bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var list = new List<Result<T>>();

            foreach (var r in source)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (r.IsFailure)
                {
                    // Preserve original failure
                    list.Add(r);
                    continue;
                }

                bool keep = await predicate(r.Value, cancellationToken)
                    .ConfigureAwait(false);

                list.Add(
                    keep
                        ? r
                        : Result<T>.Fail(r.Value, "Filtered out")
                );
            }

            return list;
        }
    }
}
