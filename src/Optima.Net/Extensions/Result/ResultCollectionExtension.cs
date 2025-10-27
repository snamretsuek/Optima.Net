using Optima.Net.Result;

namespace Optima.Net.Extensions.Result
{
    public static  class ResultCollectionExtension
    {
        /// <summary>
        /// Result<IEnumerable<T>> from many Result<T> (aggregate)
        /// If any element failed, the whole sequence fails (aggregate errors).
        /// Otherwise, you get a Result wrapping the list of successful values.
        /// This is super useful for batch operations, e.g., validating multiple inputs, creating multiple domain entities, or processing multiple tasks.
        /// </summary>
        public static Result<IEnumerable<T>> Sequence<T>(this IEnumerable<Result<T>> results)
        {
            var failures = results.Where(r => r.IsFailure).Select(r => r.Error).ToList();
            if (failures.Any())
                return Result<IEnumerable<T>>.Fail(string.Join("; ", failures));

            return Result<IEnumerable<T>>.Ok(results.Where(r => r.IsSuccess).Select(r => r.Value));
        }

        /// <summary>
        /// Result<IEnumerable<T>> from many Result<T> (aggregate)
        /// If any element failed, the whole sequence fails (aggregate errors).
        /// Otherwise, you get a Result wrapping the list of successful values.
        /// This is super useful for batch operations, e.g., validating multiple inputs, creating multiple domain entities, or processing multiple tasks.
        /// </summary>
        public static async Task<Result<IEnumerable<T>>> SequenceAsync<T>(
            this IEnumerable<Task<Result<T>>> tasks,
            CancellationToken cancellationToken = default)
        {
            // Throw if the operation was canceled before starting
            cancellationToken.ThrowIfCancellationRequested();

            // Wrap each task so it observes the cancellation token
            var wrappedTasks = tasks.Select(async t =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await t.ConfigureAwait(false);
            });

            var results = await Task.WhenAll(wrappedTasks).ConfigureAwait(false);

            return results.Sequence(); // use the synchronous Sequence helper
        }




        /// <summary>
        /// Filters a collection of Result<T> down to successes that meet a predicate:
        /// </summary>
        public static IEnumerable<Result<T>> Where<T>(
            this IEnumerable<Result<T>> results,
            Func<T, bool> predicate) =>
            results.Select(r => r.Bind(v => predicate(v) ? Result<T>.Ok(v) : Result<T>.Fail("Filtered out")));

        /// <summary>
        /// Filters a collection of Result<T> down to successes that meet a predicate:
        /// </summary>
        public static async Task<IEnumerable<Result<T>>> WhereAsync<T>(
            this IEnumerable<Result<T>> results,
            Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            var list = new List<Result<T>>();
            foreach (var r in results)
            {
                if (r.IsFailure)
                {
                    list.Add(Result<T>.Fail(r.Error));
                }
                else
                {
                    bool keep = await predicate(r.Value, cancellationToken);
                    list.Add(keep ? Result<T>.Ok(r.Value) : Result<T>.Fail("Filtered out"));
                }
            }
            return list;
        }

        /// <summary>
        /// Useful when you have Result<Result<T>> or IEnumerable<Result<T>> and want to collapse nested results
        /// </summary>
        public static Result<T> Flatten<T>(this Result<Result<T>> result) =>
            result.IsFailure ? Result<T>.Fail(result.Error) : result.Value;

    }
}
