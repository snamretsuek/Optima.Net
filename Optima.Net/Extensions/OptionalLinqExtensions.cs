using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optima.Net.Extensions.LINQ
{
    public static class OptionalLinqExtensions
    {
        /// <summary>
        /// LINQ support for Select. Transforms the inner value if present.
        /// Equivalent to Map.
        /// </summary>
        public static Optional<TResult> Select<T, TResult>(
            this Optional<T> optional,
            Func<T, TResult> selector)
        {
            if (!optional.HasValue)
                return Optional<TResult>.None();

            return Optional<TResult>.Some(selector(optional.Value));
        }

        /// <summary>
        /// LINQ support for SelectMany. Chains optionals without nesting.
        /// Equivalent to Bind.
        /// </summary>
        public static Optional<TResult> SelectMany<T, TIntermediate, TResult>(
            this Optional<T> optional,
            Func<T, Optional<TIntermediate>> binder,
            Func<T, TIntermediate, TResult> projector)
        {
            if (!optional.HasValue)
                return Optional<TResult>.None();

            var intermediate = binder(optional.Value);
            if (!intermediate.HasValue)
                return Optional<TResult>.None();

            return Optional<TResult>.Some(projector(optional.Value, intermediate.Value));
        }

        /// <summary>
        /// LINQ support for Where. Filters values that don't meet a condition.
        /// Equivalent to your Where extension.
        /// </summary>
        public static Optional<T> Where<T>(
            this Optional<T> optional,
            Func<T, bool> predicate)
        {
            if (!optional.HasValue)
                return Optional<T>.None();

            return predicate(optional.Value)
                ? optional
                : Optional<T>.None();
        }
    }
}
