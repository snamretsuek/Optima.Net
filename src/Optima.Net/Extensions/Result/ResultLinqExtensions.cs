using Optima.Net.Result;

namespace Optima.Net.Extensions.Result.LINQ
{
    public static class ResultLinqExtensions
    {
        /// <summary>
        /// LINQ support
        /// </summary>
        public static Result<U> Select<T, U>(this Result<T> result, Func<T, U> selector) => result.Map(selector);

        /// <summary>
        /// LINQ support
        /// </summary>
        public static Result<V> SelectMany<T, U, V>(
            this Result<T> result,
            Func<T, Result<U>> bind,
            Func<T, U, V> project) =>
            result.Bind(t => bind(t).Map(u => project(t, u)));


        /// <summary>
        /// LINQ support
        /// </summary>
        public static IEnumerable<Result<T>> Where<T>(
            this IEnumerable<Result<T>> source,
            Func<T, bool> predicate)
        {
            foreach (var r in source)
            {
                if (r.IsFailure)
                {
                    yield return r; // keep failures untouched
                }
                else if (predicate(r.Value))
                {
                    yield return r; // keep successful items that match
                }
                else
                {
                    yield return Result<T>.Fail("Filtered out"); // optional: mark filtered items as failures
                }
            }
        }
    }
}
