using Optima.Net.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optima.Net.Extensions
{
    public static class OptionalExtensions
    {
        /// <summary>
        /// Returns the value of the Optional if it exists, otherwise throws a NullValueException.
        /// </summary>
        /// <param name="optional">The Optional to extract from.</param>
        /// <param name="message">Optional custom error message.</param>
        /// <exception cref="NullValueException">Thrown when Optional has no value.</exception>
        public static T ValueOrThrow<T>(this Optional<T> optional, string? message = null)
        {
            if (optional.HasValue)
                return optional.Value;

            if (!string.IsNullOrWhiteSpace(message))
                throw new NullValueException(message);

            throw NullValueException.ForType<T>();
        }

        /// <summary>
        /// Executes the provided action if the Optional has a value, then returns the original Optional.
        /// Useful for side effects without breaking a functional pipeline.
        /// </summary>
        public static Optional<T> Tap<T>(this Optional<T> optional, Action<T> action)
        {
            if (optional.HasValue)
            {
                action(optional.Value);
            }

            return optional;
        }

        /// <summary>
        /// Executes the provided async action if the Optional has a value, then returns the original Optional.
        /// Useful for async side effects like logging, notifications, or metrics in a pipeline.
        /// </summary>
        public static async Task<Optional<T>> TapAsync<T>(this Optional<T> optional, Func<T, Task> asyncAction)
        {
            if (optional.HasValue)
            {
                await asyncAction(optional.Value);
            }

            return optional;
        }

        /// <summary>
        /// Returns the original Optional if the predicate is true; otherwise returns None.
        /// Useful for conditional filtering in a functional pipeline.
        /// </summary>
        public static Optional<T> Where<T>(this Optional<T> optional, Func<T, bool> predicate)
        {
            if (!optional.HasValue)
                return Optional<T>.None();

            return predicate(optional.Value)
                ? optional
                : Optional<T>.None();
        }
    }
}
}
