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

        public static async Task<Optional<T>> TapAsync<T>(this Optional<T> optional, Func<T, 
                CancellationToken, Task> asyncAction, CancellationToken cancellationToken = default)
        {
            if (optional.HasValue)
            {
                await asyncAction(optional.Value,cancellationToken);
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

        /// <summary>
        /// Filters an Optional asynchronously based on a predicate that returns a Task<bool>.
        /// Returns None if the Optional has no value or the predicate evaluates to false.
        /// </summary>

        public static async Task<Optional<T>> WhereAsync<T>(
            this Optional<T> optional,
            Func<T, Task<bool>> predicate)
        {
            if (!optional.HasValue)
                return Optional<T>.None();

            var result = await predicate(optional.Value);
            return result ? optional : Optional<T>.None();
        }

        public static async Task<Optional<T>> WhereAsync<T>(
            this Optional<T> optional,
            Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken=default)
        {
            if (!optional.HasValue)
                return Optional<T>.None();

            var result = await predicate(optional.Value,cancellationToken);
            return result ? optional : Optional<T>.None();
        }

        /// <summary>
        /// Returns this Optional if it has a value; otherwise returns the fallback Optional.
        /// </summary>
        public static Optional<T> Or<T>(this Optional<T> first, Optional<T> fallback)
            => first.HasValue ? first : fallback;

        /// <summary>
        /// Returns this Optional if it has a value; otherwise returns the result of the fallback factory.
        /// </summary>
        public static Optional<T> Or<T>(this Optional<T> first, Func<Optional<T>> fallbackFactory)
        {
            ArgumentNullException.ThrowIfNull(fallbackFactory);
            return first.HasValue ? first : fallbackFactory();
        }

        /// <summary>
        /// Returns this Optional if it has a value; otherwise awaits and returns the fallback Optional.
        /// </summary>

        public static async Task<Optional<T>> OrAsync<T>(
            this Optional<T> first,
            Func<Task<Optional<T>>> fallbackFactory)
        {
            ArgumentNullException.ThrowIfNull(fallbackFactory);
            return first.HasValue ? first : await fallbackFactory().ConfigureAwait(false);
        }

        public static async Task<Optional<T>> OrAsync<T>(
            this Optional<T> first,
            Func<CancellationToken, Task<Optional<T>>> fallbackFactory, CancellationToken cancellationToken=default)
        {
            ArgumentNullException.ThrowIfNull(fallbackFactory);
            return first.HasValue ? first : await fallbackFactory(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously matches on Some or None and returns the awaited result.
        /// </summary>

        public static async Task<TResult> MatchAsync<T, TResult>(
            this Optional<T> optional,
            Func<T, Task<TResult>> onSome,
            Func<Task<TResult>> onNone)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            if (optional.HasValue)
                return await onSome(optional.Value).ConfigureAwait(false);

            return await onNone().ConfigureAwait(false);
        }

        public static async Task<TResult> MatchAsync<T, TResult>(
            this Optional<T> optional,
            Func<T,CancellationToken, Task<TResult>> onSome,
            Func<CancellationToken,Task<TResult>> onNone, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            if (optional.HasValue)
                return await onSome(optional.Value, cancellationToken).ConfigureAwait(false);

            return await onNone(cancellationToken).ConfigureAwait(false);
        }

       
        public static async Task MatchAsync<T>(
            this Optional<T> optional,
            Func<T, Task> onSome,
            Func<Task> onNone)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            if (optional.HasValue)
                await onSome(optional.Value).ConfigureAwait(false);
            else
                await onNone().ConfigureAwait(false);
        }

        public static async Task MatchAsync<T>(
           this Optional<T> optional,
           Func<T, CancellationToken, Task> onSome,
           Func<CancellationToken, Task> onNone, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            if (optional.HasValue)
                await onSome(optional.Value,cancellationToken).ConfigureAwait(false);
            else
                await onNone(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs asynchronous side effects based on Some/None and returns the original Optional for continued chaining.
        /// </summary>
        public static async Task<Optional<T>> MatchAsync<T>(
            this Optional<T> optional,
            Func<T, Task> onSome,
            Func<Task> onNone,
            bool configureAwait = false)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            if (optional.HasValue)
                await onSome(optional.Value).ConfigureAwait(configureAwait);
            else
                await onNone().ConfigureAwait(configureAwait);

            return optional;
        }

        public static async Task<Optional<T>> MatchAsync<T>(
            this Optional<T> optional,
            Func<T, CancellationToken, Task> onSome,
            Func<CancellationToken, Task> onNone,
            bool configureAwait = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(onSome);
            ArgumentNullException.ThrowIfNull(onNone);

            if (optional.HasValue)
                await onSome(optional.Value, cancellationToken).ConfigureAwait(configureAwait);
            else
                await onNone(cancellationToken).ConfigureAwait(configureAwait);

            return optional;
        }

        /// <summary>
        /// Asynchronously maps the inner value of an Optional to a new Optional.
        /// </summary>
        public static async Task<Optional<TResult>> MapAsync<T, TResult>(
            this Optional<T> optional,
            Func<T, Task<TResult>> asyncMapper)
        {
            ArgumentNullException.ThrowIfNull(asyncMapper);

            if (!optional.HasValue)
                return Optional<TResult>.None();

            var result = await asyncMapper(optional.Value).ConfigureAwait(false);
            return Optional<TResult>.Some(result);
        }

        public static async Task<Optional<TResult>> MapAsync<T, TResult>(
            this Optional<T> optional,
            Func<T, CancellationToken, Task<TResult>> asyncMapper, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(asyncMapper);

            if (!optional.HasValue)
                return Optional<TResult>.None();

            var result = await asyncMapper(optional.Value,cancellationToken).ConfigureAwait(false);
            return Optional<TResult>.Some(result);
        }

        /// <summary>
        /// Asynchronously binds an Optional to a new Optional, flattening the result.
        /// </summary>
        public static async Task<Optional<TResult>> BindAsync<T, TResult>(
            this Optional<T> optional,
            Func<T, Task<Optional<TResult>>> asyncBinder)
        {
            ArgumentNullException.ThrowIfNull(asyncBinder);

            if (!optional.HasValue)
                return Optional<TResult>.None();

            return await asyncBinder(optional.Value).ConfigureAwait(false);
        }

        public static async Task<Optional<TResult>> BindAsync<T, TResult>(
            this Optional<T> optional,
            Func<T,CancellationToken, Task<Optional<TResult>>> asyncBinder, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(asyncBinder);

            if (!optional.HasValue)
                return Optional<TResult>.None();

            return await asyncBinder(optional.Value,cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to get the value of the Optional.
        /// Returns true if a value exists, false otherwise.
        /// </summary>
        public static bool TryGetValue<T>(this Optional<T> optional, out T value)
        {
            if (optional.HasValue)
            {
                value = optional.Value;
                return true;
            }

            value = default!;
            return false;
        }

        /// <summary>
        /// Combines two Optionals into one Optional using a result selector function.
        /// Returns None if either Optional is empty.
        /// </summary>
        public static Optional<TResult> Zip<T1, T2, TResult>(
            this Optional<T1> first,
            Optional<T2> second,
            Func<T1, T2, TResult> resultSelector)
        {
            ArgumentNullException.ThrowIfNull(resultSelector);

            if (!first.HasValue || !second.HasValue)
                return Optional<TResult>.None();

            return Optional<TResult>.Some(resultSelector(first.Value, second.Value));
        }

        /// <summary>
        /// Asynchronously zips two Optionals, producing an Optional with the combined result.
        /// Returns None if either Optional is empty.
        /// </summary>
        public static async Task<Optional<TResult>> ZipAsync<T1, T2, TResult>(
            this Task<Optional<T1>> firstTask,
            Task<Optional<T2>> secondTask,
            Func<T1, T2, TResult> resultSelector)
        {
            ArgumentNullException.ThrowIfNull(firstTask);
            ArgumentNullException.ThrowIfNull(secondTask);
            ArgumentNullException.ThrowIfNull(resultSelector);

            var first = await firstTask.ConfigureAwait(false);
            var second = await secondTask.ConfigureAwait(false);

            if (!first.HasValue || !second.HasValue)
                return Optional<TResult>.None();

            return Optional<TResult>.Some(resultSelector(first.Value, second.Value));
        }

        public static async Task<Optional<TResult>> ZipAsync<T1, T2, TResult>(
            this Task<Optional<T1>> firstTask,
            Task<Optional<T2>> secondTask,
            Func<T1, T2, TResult> resultSelector,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(firstTask);
            ArgumentNullException.ThrowIfNull(secondTask);
            ArgumentNullException.ThrowIfNull(resultSelector);

            // Await both with cancellation
            var first = await firstTask.WaitAsync(cancellationToken).ConfigureAwait(false);
            var second = await secondTask.WaitAsync(cancellationToken).ConfigureAwait(false);

            if (!first.HasValue || !second.HasValue)
                return Optional<TResult>.None();

            cancellationToken.ThrowIfCancellationRequested();

            return Optional<TResult>.Some(resultSelector(first.Value, second.Value));
        }

        /// <summary>
        /// Converts an Optional into a 0-or-1 IEnumerable.
        /// </summary>
        public static IEnumerable<T> ToEnumerable<T>(this Optional<T> optional)
        {
            if (optional.HasValue)
                yield return optional.Value;
        }
    }
}


