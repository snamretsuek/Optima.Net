using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optima.Net.Extensions
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
