using Optima.Net.Result;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Optima.Net.Extensions.Result
{
    /// <summary>
    /// Provides extensions for composing, transforming, inspecting and validating results.
    /// </summary>
    /// <remarks>
    /// Exceptions thrown by delegates propagate to the caller and are not converted
    /// into failure results.
    /// Cancellation tokens are passed to delegates. Validation and failure hook
    /// extensions also check cancellation before invoking a delegate.
    /// </remarks>
    public static partial class ResultExtensions
    {
        // Bind

        /// <summary>
        /// Invokes a function that returns a new result when the current result succeeds.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The new value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The function to invoke on success.</param>
        /// <returns>
        /// The result returned by the function, or a failure containing the current error.
        /// </returns>
        /// <remarks>
        /// On failure, the function is not invoked and the current value is not accessed
        /// or carried into the new result.
        /// </remarks>
        public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> func) =>
            result.IsFailure
                ? Result<U>.Fail(result.Error)
                : func(result.Value);

        /// <summary>
        /// Asynchronously invokes a function that returns a new result when the current
        /// result succeeds.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The new value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The asynchronous function to invoke on success.</param>
        /// <returns>
        /// A task containing the function result, or a failure containing the current error.
        /// </returns>
        /// <remarks>
        /// On failure, the function is not invoked and the current value is not accessed
        /// or carried into the new result.
        /// </remarks>
        public static async Task<Result<U>> BindAsync<T, U>(
            this Result<T> result,
            Func<T, Task<Result<U>>> func) => result.IsFailure
                ? Result<U>.Fail(result.Error)
                : await func(result.Value).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously invokes a function with a cancellation token when the current
        /// result succeeds.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The new value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The asynchronous function to invoke on success.</param>
        /// <param name="cancellationToken">The cancellation token passed to the function.</param>
        /// <returns>
        /// A task containing the function result, or a failure containing the current error.
        /// </returns>
        /// <remarks>
        /// On failure, the function is not invoked and the current value is not accessed
        /// or carried into the new result.
        /// </remarks>
        public static async Task<Result<U>> BindAsync<T, U>(
            this Result<T> result,
            Func<T, CancellationToken, Task<Result<U>>> func,
            CancellationToken cancellationToken = default) =>
            result.IsFailure
                ? Result<U>.Fail(result.Error)
                : await func(result.Value, cancellationToken).ConfigureAwait(false);

        // Map

        /// <summary>
        /// Transforms a successful value and wraps the transformed value in a success result.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The transformed value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The function used to transform the value.</param>
        /// <returns>
        /// A success containing the transformed value, or a failure containing the current error.
        /// </returns>
        /// <remarks>
        /// On failure, the function is not invoked and the current value is not accessed
        /// or carried into the new result.
        /// </remarks>
        public static Result<U> Map<T, U>(
            this Result<T> result,
            Func<T, U> func) => result.IsFailure
                ? Result<U>.Fail(result.Error)
                : Result<U>.Ok(func(result.Value));

        /// <summary>
        /// Asynchronously transforms a successful value and wraps it in a success result.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The transformed value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The asynchronous function used to transform the value.</param>
        /// <returns>
        /// A task containing a success with the transformed value, or a failure
        /// containing the current error.
        /// </returns>
        /// <remarks>
        /// On failure, the function is not invoked and the current value is not accessed
        /// or carried into the new result.
        /// </remarks>
        public static async Task<Result<U>> MapAsync<T, U>(
            this Result<T> result,
            Func<T, Task<U>> func) => result.IsFailure
                ? Result<U>.Fail(result.Error)
                : Result<U>.Ok(await func(result.Value).ConfigureAwait(false));

        /// <summary>
        /// Asynchronously transforms a successful value using a function that accepts
        /// a cancellation token.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The transformed value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The asynchronous function used to transform the value.</param>
        /// <param name="cancellationToken">The cancellation token passed to the function.</param>
        /// <returns>
        /// A task containing a success with the transformed value, or a failure
        /// containing the current error.
        /// </returns>
        /// <remarks>
        /// On failure, the function is not invoked and the current value is not accessed
        /// or carried into the new result.
        /// </remarks>
        public static async Task<Result<U>> MapAsync<T, U>(
            this Result<T> result,
            Func<T, CancellationToken, Task<U>> func,
            CancellationToken cancellationToken = default) =>
            result.IsFailure
                ? Result<U>.Fail(result.Error)
                : Result<U>.Ok(await func(result.Value, cancellationToken).ConfigureAwait(false));

        // Tap

        /// <summary>
        /// Executes an action on a successful value and returns the original result.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The action to execute on success.</param>
        /// <returns>The original result.</returns>
        /// <remarks>
        /// Useful for side effects such as logging. The action is skipped on failure.
        /// </remarks>
        public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSuccess)
                action(result.Value);

            return result;
        }

        /// <summary>
        /// Asynchronously executes an action on a successful value and returns the
        /// original result.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The asynchronous action to execute on success.</param>
        /// <returns>A task containing the original result.</returns>
        /// <remarks>The action is skipped on failure.</remarks>
        public static async Task<Result<T>> TapAsync<T>(
            this Result<T> result,
            Func<T, Task> action)
        {
            if (result.IsSuccess)
                await action(result.Value).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Asynchronously executes an action with a cancellation token on a successful
        /// value and returns the original result.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The asynchronous action to execute on success.</param>
        /// <param name="cancellationToken">The cancellation token passed to the action.</param>
        /// <returns>A task containing the original result.</returns>
        /// <remarks>The action is skipped on failure.</remarks>
        public static async Task<Result<T>> TapAsync<T>(
            this Result<T> result,
            Func<T, CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            if (result.IsSuccess)
                await action(result.Value, cancellationToken).ConfigureAwait(false);

            return result;
        }

        // Failure hook

        /// <summary>
        /// Executes an action on the error when the result fails.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The action to execute on failure.</param>
        /// <returns>The original result.</returns>
        /// <remarks>
        /// The action is skipped on success. This method does not recover from failure
        /// or replace the error.
        /// </remarks>
        public static Result<T> OnFailure<T>(this Result<T> result, Action<string> action)
        {
            if (result.IsFailure)
                action(result.Error);

            return result;
        }

        // Match

        /// <summary>
        /// Produces a value by invoking the function that matches the result state.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TResult">The output type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="onSuccess">The function invoked with the value on success.</param>
        /// <param name="onFailure">The function invoked with the error on failure.</param>
        /// <returns>The value returned by the selected function.</returns>
        /// <remarks>Only the function matching the result state is invoked.</remarks>
        public static TResult Match<T, TResult>(
            this Result<T> result,
            Func<T, TResult> onSuccess,
            Func<string, TResult> onFailure) =>
            result.IsSuccess
                ? onSuccess(result.Value)
                : onFailure(result.Error);

        /// <summary>
        /// Asynchronously produces a value by invoking the function that matches
        /// the result state.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TResult">The output type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="onSuccess">The asynchronous function invoked on success.</param>
        /// <param name="onFailure">The asynchronous function invoked on failure.</param>
        /// <returns>A task containing the value returned by the selected function.</returns>
        /// <remarks>Only the function matching the result state is invoked.</remarks>
        public static async Task<TResult> MatchAsync<T, TResult>(
            this Result<T> result,
            Func<T, Task<TResult>> onSuccess,
            Func<string, Task<TResult>> onFailure) =>
            result.IsSuccess
                ? await onSuccess(result.Value).ConfigureAwait(false)
                : await onFailure(result.Error).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously produces a value by invoking the function that matches
        /// the result state and passing it a cancellation token.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TResult">The output type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="onSuccess">The asynchronous function invoked on success.</param>
        /// <param name="onFailure">The asynchronous function invoked on failure.</param>
        /// <param name="cancellationToken">
        /// The cancellation token passed to the selected function.
        /// </param>
        /// <returns>A task containing the value returned by the selected function.</returns>
        /// <remarks>Only the function matching the result state is invoked.</remarks>
        public static async Task<TResult> MatchAsync<T, TResult>(
            this Result<T> result,
            Func<T, CancellationToken, Task<TResult>> onSuccess,
            Func<string, CancellationToken, Task<TResult>> onFailure,
            CancellationToken cancellationToken = default) =>
            result.IsSuccess
                ? await onSuccess(result.Value, cancellationToken).ConfigureAwait(false)
                : await onFailure(result.Error, cancellationToken).ConfigureAwait(false);

        // Validation

        /// <summary>
        /// Runs the next validator only when the current result succeeds.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="validate">The validator to invoke on success.</param>
        /// <returns>
        /// The validator result, or the original result if it has already failed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The result or validator is null.
        /// </exception>
        /// <remarks>
        /// A chain of validators stops executing at the first failure.
        /// Existing failures are returned without accessing their values.
        /// </remarks>
        public static Result<T> Then<T>(this Result<T> result, Func<T, Result<T>> validate)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (validate is null)
                throw new ArgumentNullException(nameof(validate));

            return result.IsFailure
                ? result
                : validate(result.Value);
        }

        /// <summary>
        /// Checks a condition on a successful value and returns a failure if it is not met.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="predicate">The condition that the value must satisfy.</param>
        /// <param name="error">The error to return when the condition is false.</param>
        /// <returns>
        /// The original result if it has already failed or the condition is true.
        /// Otherwise, a failure containing the current value and the supplied error.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The result or predicate is null.
        /// </exception>
        /// <remarks>The predicate is not invoked when the result has already failed.</remarks>
        public static Result<T> Ensure<T>(
            this Result<T> result,
            Func<T, bool> predicate,
            string error)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (result.IsFailure)
                return result;

            return predicate(result.Value)
                ? result
                : Result<T>.Fail(result.Value, error);
        }

        /// <summary>
        /// Runs all supplied validators on a successful value and combines their errors.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="validators">The independent validators to execute in order.</param>
        /// <returns>
        /// The original result if it has already failed or no validators fail.
        /// Otherwise, a failure containing the original value and all validation errors,
        /// separated by the environment newline.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The result or validators array is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The validators array contains a null entry.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// A validator returns null.
        /// </exception>
        /// <remarks>
        /// Validators execute sequentially against the same original value.
        /// Returned failures do not prevent subsequent validators from running.
        /// Replacement values returned by validators are ignored.
        /// Validators should perform independent checks and should not modify the input.
        /// Exceptions stop execution and propagate to the caller.
        /// An empty validators array returns the original result.
        /// Validator arguments are checked even when the result has already failed.
        /// </remarks>
        public static Result<T> ValidateAll<T>(
            this Result<T> result,
            params Func<T, Result<T>>[] validators)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (validators is null)
                throw new ArgumentNullException(nameof(validators));

            // Check every validator before executing any validation.
            for (var i = 0; i < validators.Length; i++)
            {
                if (validators[i] is null)
                {
                    throw new ArgumentException(
                        $"Validator at index {i} cannot be null.",
                        nameof(validators));
                }
            }

            // Preserve an existing failure without accessing its value.
            if (result.IsFailure)
                return result;

            var value = result.Value;
            var errors = new System.Collections.Generic.List<string>();

            // Collect returned failures without stopping at the first error.
            foreach (var validate in validators)
            {
                var validation = validate(value)
                    ?? throw new InvalidOperationException(
                        "A validator returned null.");

                if (validation.IsFailure)
                    errors.Add(validation.Error);
            }

            // Retain the original value and combine errors in validation order.
            return errors.Count == 0
                ? result
                : Result<T>.Fail(
                    value,
                    string.Join(Environment.NewLine, errors));
        }

        // Asynchronous validation and failure handling

        /// <summary>
        /// Asynchronously executes an action on failure and returns the original result.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The asynchronous action invoked with the error on failure.</param>
        /// <returns>A task containing the original result.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, action.
        /// </exception>
        public static Task<Result<T>> OnFailureAsync<T>(
            this Result<T> result,
            Func<string, Task> action)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (action is null)
                throw new ArgumentNullException(nameof(action));

            return result.OnFailureAsync((error, _) => action(error));
        }

        /// <summary>
        /// Asynchronously executes an action on failure with cancellation support.
        /// A successful result is returned without checking cancellation.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The asynchronous action invoked with the error on failure.</param>
        /// <param name="cancellationToken">The cancellation token checked before invoking the selected delegate and passed to it.</param>
        /// <returns>A task containing the original result.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, action.
        /// </exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<T>> OnFailureAsync<T>(
            this Result<T> result,
            Func<string, CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (action is null)
                throw new ArgumentNullException(nameof(action));

            if (result.IsSuccess)
                return result;

            cancellationToken.ThrowIfCancellationRequested();

            await action(result.Error, cancellationToken).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Asynchronously runs the next validator on success, or returns
        /// the original failure.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="validate">The asynchronous validator invoked with the value on success.</param>
        /// <returns>A task containing the validator result, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, validate.
        /// </exception>
        /// <exception cref="InvalidOperationException">A validator returns null.</exception>
        public static Task<Result<T>> ThenAsync<T>(
            this Result<T> result,
            Func<T, Task<Result<T>>> validate)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (validate is null)
                throw new ArgumentNullException(nameof(validate));

            return result.ThenAsync((value, _) => validate(value));
        }

        /// <summary>
        /// Asynchronously runs the next validator with cancellation support.
        /// An existing failure is returned without checking cancellation.
        /// A null validator result causes an InvalidOperationException.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="validate">The asynchronous validator invoked with the value on success.</param>
        /// <param name="cancellationToken">The cancellation token checked before invoking the selected delegate and passed to it.</param>
        /// <returns>A task containing the validator result, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, validate.
        /// </exception>
        /// <exception cref="InvalidOperationException">A validator returns null.</exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<T>> ThenAsync<T>(
            this Result<T> result,
            Func<T, CancellationToken, Task<Result<T>>> validate,
            CancellationToken cancellationToken = default)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (validate is null)
                throw new ArgumentNullException(nameof(validate));

            if (result.IsFailure)
                return result;

            cancellationToken.ThrowIfCancellationRequested();

            return await validate(result.Value, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    "A validator returned null.");
        }

        /// <summary>
        /// Asynchronously checks a condition on success.
        /// Returns a failure with the original value and supplied error if false.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="predicate">The asynchronous condition evaluated on success.</param>
        /// <param name="error">The error returned when the condition is false.</param>
        /// <returns>A task containing the original result if already failed or the condition is true; otherwise, a failure containing the original value and supplied error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, predicate.
        /// </exception>
        public static Task<Result<T>> EnsureAsync<T>(
            this Result<T> result,
            Func<T, Task<bool>> predicate,
            string error)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return result.EnsureAsync(
                (value, _) => predicate(value),
                error);
        }

        /// <summary>
        /// Asynchronously checks a condition with cancellation support.
        /// Existing failures are returned without checking cancellation.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="predicate">The asynchronous condition evaluated on success.</param>
        /// <param name="error">The error returned when the condition is false.</param>
        /// <param name="cancellationToken">The cancellation token checked before invoking the selected delegate and passed to it.</param>
        /// <returns>A task containing the original result if already failed or the condition is true; otherwise, a failure containing the original value and supplied error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, predicate.
        /// </exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<T>> EnsureAsync<T>(
            this Result<T> result,
            Func<T, CancellationToken, Task<bool>> predicate,
            string error,
            CancellationToken cancellationToken = default)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (result.IsFailure)
                return result;

            cancellationToken.ThrowIfCancellationRequested();

            var value = result.Value;
            var isValid = await predicate(value, cancellationToken)
                .ConfigureAwait(false);

            return isValid
                ? result
                : Result<T>.Fail(value, error);
        }

        /// <summary>
        /// Runs asynchronous validators sequentially and combines their returned errors.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <remarks>
        /// Errors are joined with the environment newline in validation order.
        /// The original input value is retained. Replacement validator values are ignored.
        /// Exceptions stop execution and propagate to the caller.
        /// </remarks>
        /// <param name="result">The current result.</param>
        /// <param name="validators">The independent asynchronous validators to execute sequentially in the supplied order.</param>
        /// <returns>A task containing the original result if already failed or no validators fail; otherwise, a failure containing the original value and errors joined with the environment newline.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, validators.
        /// </exception>
        /// <exception cref="ArgumentException">The validators array contains a null entry.</exception>
        /// <exception cref="InvalidOperationException">A validator returns null.</exception>
        public static Task<Result<T>> ValidateAllAsync<T>(
            this Result<T> result,
            params Func<T, Task<Result<T>>>[] validators)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (validators is null)
                throw new ArgumentNullException(nameof(validators));

            var adapted =
                new Func<T, CancellationToken, Task<Result<T>>>[
                    validators.Length];

            for (var i = 0; i < validators.Length; i++)
            {
                var validator = validators[i];

                if (validator is null)
                {
                    throw new ArgumentException(
                        $"Validator at index {i} cannot be null.",
                        nameof(validators));
                }

                adapted[i] = (value, _) => validator(value);
            }

            return result.ValidateAllAsync(
                CancellationToken.None,
                adapted);
        }

        /// <summary>
        /// Runs asynchronous validators sequentially with cancellation support
        /// and combines their returned errors.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="cancellationToken">
        /// Checked before each validator and passed to that validator.
        /// </param>
        /// <param name="validators">The independent validators to execute in order.</param>
        /// <returns>
        /// The original result if already failed or no validators fail.
        /// Otherwise, a failure with the original value and errors joined with the environment newline.
        /// </returns>
        /// <remarks>
        /// Validators receive the same original value and should not modify it.
        /// Replacement values returned by validators are ignored.
        /// Returned failures do not prevent subsequent validators from running.
        /// Exceptions and cancellation stop execution without returning partial errors.
        /// An existing failure or empty validators array is returned unchanged
        /// without checking cancellation.
        /// A null validator result causes an InvalidOperationException.
        /// Arguments are checked even when the result has already failed.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, validators.
        /// </exception>
        /// <exception cref="ArgumentException">The validators array contains a null entry.</exception>
        /// <exception cref="InvalidOperationException">A validator returns null.</exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<T>> ValidateAllAsync<T>(
            this Result<T> result,
            CancellationToken cancellationToken,
            params Func<T, CancellationToken, Task<Result<T>>>[] validators)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (validators is null)
                throw new ArgumentNullException(nameof(validators));

            // Check all validators before executing any validation.
            for (var i = 0; i < validators.Length; i++)
            {
                if (validators[i] is null)
                {
                    throw new ArgumentException(
                        $"Validator at index {i} cannot be null.",
                        nameof(validators));
                }
            }

            if (result.IsFailure)
                return result;

            var value = result.Value;
            var errors = new System.Collections.Generic.List<string>();

            foreach (var validate in validators)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var validation = await validate(value, cancellationToken)
                    .ConfigureAwait(false)
                    ?? throw new InvalidOperationException(
                        "A validator returned null.");

                if (validation.IsFailure)
                    errors.Add(validation.Error);
            }

            return errors.Count == 0
                ? result
                : Result<T>.Fail(
                    value,
                    string.Join(Environment.NewLine, errors));
        }
    }
}