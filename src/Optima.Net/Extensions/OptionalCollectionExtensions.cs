namespace Optima.Net.Extensions.Collections
{
    public static class OptionalCollectionExtensions
    {
        /// <summary>
        /// Filters a sequence of Optionals, returning only those that have a value
        /// and satisfy the predicate.
        /// </summary>
        public static IEnumerable<Optional<T>> Where<T>(
            this IEnumerable<Optional<T>> optionals,
            Func<T, bool> predicate)
        {
            return optionals
                .Where(o => o.HasValue && predicate(o.Value));
        }

        /// <summary>
        /// Asynchronously filters a sequence of Optionals based on an async predicate.
        /// Returns only Optionals that have a value and for which the predicate returns true.
        /// </summary>
        public static async Task<IEnumerable<Optional<T>>> WhereAsync<T>(
            this IEnumerable<Optional<T>> optionals,
            Func<T, Task<bool>> predicate)
        {
            var tasks = optionals.Select(async o =>
            {
                if (!o.HasValue)
                    return Optional<T>.None();

                var keep = await predicate(o.Value);
                return keep ? o : Optional<T>.None();
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r.HasValue);
        }

        public static async Task<IEnumerable<Optional<T>>> WhereAsync<T>(
            this IEnumerable<Optional<T>> optionals,
            Func<T,CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
        {
            var tasks = optionals.Select(async o =>
            {
                if (!o.HasValue)
                    return Optional<T>.None();

                var keep = await predicate(o.Value,default);
                return keep ? o : Optional<T>.None();
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r.HasValue);
        }


        /// <summary>
        /// Flattens a sequence of Optionals to only the contained values.
        /// </summary>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<Optional<T>> optionals)
        {
            return optionals
                .Where(o => o.HasValue)
                .Select(o => o.Value);
        }
    }
}
