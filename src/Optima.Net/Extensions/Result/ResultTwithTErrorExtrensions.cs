using Optima.Net.Result;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Optima.Net.Extensions.Result
{
    public static partial class ResultExtensions
    {
        // Bind

        /// <summary>
        /// Invokes the function on success, or forwards the existing error.
        /// The original value is not carried into a forwarded failure.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The output value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The function invoked with the value on success.</param>
        /// <returns>The function result, or a failure containing the existing error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, func.
        /// </exception>
        public static Result<U, TError> Bind<T, U, TError>(
            this Result<T, TError> result,
            Func<T, Result<U, TError>> func)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (func is null)
                throw new ArgumentNullException(nameof(func));

            return result.IsFailure
                ? Result<U, TError>.Fail(result.Error)
                : func(result.Value);
        }

        /// <summary>
        /// Asynchronously invokes the function on success, or forwards the existing error.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The output value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The asynchronous function invoked with the value on success.</param>
        /// <returns>A task containing the function result, or a failure containing the existing error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, func.
        /// </exception>
        public static Task<Result<U, TError>> BindAsync<T, U, TError>(
            this Result<T, TError> result,
            Func<T, Task<Result<U, TError>>> func)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (func is null)
                throw new ArgumentNullException(nameof(func));

            return result.BindAsync((value, _) => func(value));
        }

        /// <summary>
        /// Asynchronously invokes the function on success with cancellation support.
        /// An existing failure is forwarded without checking cancellation.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The output value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The asynchronous function invoked with the value on success.</param>
        /// <param name="cancellationToken">The cancellation token checked before invoking the selected delegate and passed to it.</param>
        /// <returns>A task containing the function result, or a failure containing the existing error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, func.
        /// </exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<U, TError>> BindAsync<T, U, TError>(
            this Result<T, TError> result,
            Func<T, CancellationToken, Task<Result<U, TError>>> func,
            CancellationToken cancellationToken = default)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (func is null)
                throw new ArgumentNullException(nameof(func));

            if (result.IsFailure)
                return Result<U, TError>.Fail(result.Error);

            cancellationToken.ThrowIfCancellationRequested();

            return await func(result.Value, cancellationToken)
                .ConfigureAwait(false);
        }

        // Map

        /// <summary>
        /// Transforms a successful value, or forwards the existing error.
        /// The original value is not carried into a forwarded failure.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The output value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The function invoked with the value on success.</param>
        /// <returns>A success containing the transformed value, or a failure containing the existing error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, func.
        /// </exception>
        public static Result<U, TError> Map<T, U, TError>(
            this Result<T, TError> result,
            Func<T, U> func)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (func is null)
                throw new ArgumentNullException(nameof(func));

            return result.IsFailure
                ? Result<U, TError>.Fail(result.Error)
                : Result<U, TError>.Ok(func(result.Value));
        }

        /// <summary>
        /// Asynchronously transforms a successful value, or forwards the existing error.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The output value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The asynchronous function invoked with the value on success.</param>
        /// <returns>A task containing a success containing the transformed value, or a failure containing the existing error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, func.
        /// </exception>
        public static Task<Result<U, TError>> MapAsync<T, U, TError>(
            this Result<T, TError> result,
            Func<T, Task<U>> func)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (func is null)
                throw new ArgumentNullException(nameof(func));

            return result.MapAsync((value, _) => func(value));
        }

        /// <summary>
        /// Asynchronously transforms a successful value with cancellation support.
        /// An existing failure is forwarded without checking cancellation.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="U">The output value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="func">The asynchronous function invoked with the value on success.</param>
        /// <param name="cancellationToken">The cancellation token checked before invoking the selected delegate and passed to it.</param>
        /// <returns>A task containing a success containing the transformed value, or a failure containing the existing error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, func.
        /// </exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<U, TError>> MapAsync<T, U, TError>(
            this Result<T, TError> result,
            Func<T, CancellationToken, Task<U>> func,
            CancellationToken cancellationToken = default)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (func is null)
                throw new ArgumentNullException(nameof(func));

            if (result.IsFailure)
                return Result<U, TError>.Fail(result.Error);

            cancellationToken.ThrowIfCancellationRequested();

            var value = await func(result.Value, cancellationToken)
                .ConfigureAwait(false);

            return Result<U, TError>.Ok(value);
        }

        // Tap

        /// <summary>
        /// Executes an action on success and returns the original result.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The action invoked with the value on success.</param>
        /// <returns>The original result.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, action.
        /// </exception>
        public static Result<T, TError> Tap<T, TError>(
            this Result<T, TError> result,
            Action<T> action)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (action is null)
                throw new ArgumentNullException(nameof(action));

            if (result.IsSuccess)
                action(result.Value);

            return result;
        }

        /// <summary>
        /// Asynchronously executes an action on success and returns the original result.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The asynchronous action invoked with the value on success.</param>
        /// <returns>A task containing the original result.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, action.
        /// </exception>
        public static Task<Result<T, TError>> TapAsync<T, TError>(
            this Result<T, TError> result,
            Func<T, Task> action)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (action is null)
                throw new ArgumentNullException(nameof(action));

            return result.TapAsync((value, _) => action(value));
        }

        /// <summary>
        /// Asynchronously executes an action on success with cancellation support.
        /// An existing failure is returned without checking cancellation.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The asynchronous action invoked with the value on success.</param>
        /// <param name="cancellationToken">The cancellation token checked before invoking the selected delegate and passed to it.</param>
        /// <returns>A task containing the original result.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, action.
        /// </exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<T, TError>> TapAsync<T, TError>(
            this Result<T, TError> result,
            Func<T, CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (action is null)
                throw new ArgumentNullException(nameof(action));

            if (result.IsFailure)
                return result;

            cancellationToken.ThrowIfCancellationRequested();

            await action(result.Value, cancellationToken).ConfigureAwait(false);

            return result;
        }

        // Failure hook

        /// <summary>
        /// Executes an action on failure and returns the original result.
        /// This method does not recover from failure or replace the error.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The action invoked with the error on failure.</param>
        /// <returns>The original result.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, action.
        /// </exception>
        public static Result<T, TError> OnFailure<T, TError>(
            this Result<T, TError> result,
            Action<TError> action)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (action is null)
                throw new ArgumentNullException(nameof(action));

            if (result.IsFailure)
                action(result.Error);

            return result;
        }

        /// <summary>
        /// Asynchronously executes an action on failure and returns the original result.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The asynchronous action invoked with the error on failure.</param>
        /// <returns>A task containing the original result.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, action.
        /// </exception>
        public static Task<Result<T, TError>> OnFailureAsync<T, TError>(
            this Result<T, TError> result,
            Func<TError, Task> action)
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
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="action">The asynchronous action invoked with the error on failure.</param>
        /// <param name="cancellationToken">The cancellation token checked before invoking the selected delegate and passed to it.</param>
        /// <returns>A task containing the original result.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, action.
        /// </exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<T, TError>> OnFailureAsync<T, TError>(
            this Result<T, TError> result,
            Func<TError, CancellationToken, Task> action,
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

        // Match

        /// <summary>
        /// Invokes the function matching the result state and returns its output.
        /// Only one function is invoked.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <typeparam name="TResult">The output type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="onSuccess">The function invoked with the value on success.</param>
        /// <param name="onFailure">The function invoked with the error on failure.</param>
        /// <returns>The output of the selected function.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, onSuccess, onFailure.
        /// </exception>
        public static TResult Match<T, TError, TResult>(
            this Result<T, TError> result,
            Func<T, TResult> onSuccess,
            Func<TError, TResult> onFailure)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (onSuccess is null)
                throw new ArgumentNullException(nameof(onSuccess));

            if (onFailure is null)
                throw new ArgumentNullException(nameof(onFailure));

            return result.IsSuccess
                ? onSuccess(result.Value)
                : onFailure(result.Error);
        }

        /// <summary>
        /// Asynchronously invokes the function matching the result state.
        /// Only one function is invoked.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <typeparam name="TResult">The output type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="onSuccess">The asynchronous function invoked with the value on success.</param>
        /// <param name="onFailure">The asynchronous function invoked with the error on failure.</param>
        /// <returns>A task containing the output of the selected function.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, onSuccess, onFailure.
        /// </exception>
        public static Task<TResult> MatchAsync<T, TError, TResult>(
            this Result<T, TError> result,
            Func<T, Task<TResult>> onSuccess,
            Func<TError, Task<TResult>> onFailure)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (onSuccess is null)
                throw new ArgumentNullException(nameof(onSuccess));

            if (onFailure is null)
                throw new ArgumentNullException(nameof(onFailure));

            return result.MatchAsync(
                (value, _) => onSuccess(value),
                (error, _) => onFailure(error));
        }

        /// <summary>
        /// Asynchronously invokes the function matching the result state with
        /// cancellation support. Cancellation is checked before either function runs.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <typeparam name="TResult">The output type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="onSuccess">The asynchronous function invoked with the value on success.</param>
        /// <param name="onFailure">The asynchronous function invoked with the error on failure.</param>
        /// <param name="cancellationToken">The cancellation token checked before invoking the selected delegate and passed to it.</param>
        /// <returns>A task containing the output of the selected function.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, onSuccess, onFailure.
        /// </exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<TResult> MatchAsync<T, TError, TResult>(
            this Result<T, TError> result,
            Func<T, CancellationToken, Task<TResult>> onSuccess,
            Func<TError, CancellationToken, Task<TResult>> onFailure,
            CancellationToken cancellationToken = default)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (onSuccess is null)
                throw new ArgumentNullException(nameof(onSuccess));

            if (onFailure is null)
                throw new ArgumentNullException(nameof(onFailure));

            cancellationToken.ThrowIfCancellationRequested();

            return result.IsSuccess
                ? await onSuccess(result.Value, cancellationToken)
                    .ConfigureAwait(false)
                : await onFailure(result.Error, cancellationToken)
                    .ConfigureAwait(false);
        }

        // Validation

        /// <summary>
        /// Runs the next validator on success, or returns the original failure.
        /// A chain stops executing validators at the first failure.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="validate">The validator invoked with the value on success.</param>
        /// <returns>The validator result, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, validate.
        /// </exception>
        /// <exception cref="InvalidOperationException">A validator returns null.</exception>
        public static Result<T, TError> Then<T, TError>(
            this Result<T, TError> result,
            Func<T, Result<T, TError>> validate)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (validate is null)
                throw new ArgumentNullException(nameof(validate));

            return result.IsFailure
                ? result
                : validate(result.Value)
                    ?? throw new InvalidOperationException(
                        "A validator returned null.");
        }

        /// <summary>
        /// Asynchronously runs the next validator on success, or returns
        /// the original failure.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="validate">The asynchronous validator invoked with the value on success.</param>
        /// <returns>A task containing the validator result, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, validate.
        /// </exception>
        /// <exception cref="InvalidOperationException">A validator returns null.</exception>
        public static Task<Result<T, TError>> ThenAsync<T, TError>(
            this Result<T, TError> result,
            Func<T, Task<Result<T, TError>>> validate)
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
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="validate">The asynchronous validator invoked with the value on success.</param>
        /// <param name="cancellationToken">The cancellation token checked before invoking the selected delegate and passed to it.</param>
        /// <returns>A task containing the validator result, or the original failure.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, validate.
        /// </exception>
        /// <exception cref="InvalidOperationException">A validator returns null.</exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<T, TError>> ThenAsync<T, TError>(
            this Result<T, TError> result,
            Func<T, CancellationToken, Task<Result<T, TError>>> validate,
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
        /// Checks a condition on success.
        /// Returns a failure with the original value and supplied error if false.
        /// Existing failures are returned unchanged.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="predicate">The condition evaluated on success.</param>
        /// <param name="error">The error returned when the condition is false.</param>
        /// <returns>The original result if already failed or the condition is true; otherwise, a failure containing the original value and supplied error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, predicate.
        /// </exception>
        public static Result<T, TError> Ensure<T, TError>(
            this Result<T, TError> result,
            Func<T, bool> predicate,
            TError error)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (result.IsFailure)
                return result;

            var value = result.Value;

            return predicate(value)
                ? result
                : Result<T, TError>.Fail(value, error);
        }

        /// <summary>
        /// Asynchronously checks a condition on success.
        /// Returns a failure with the original value and supplied error if false.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="predicate">The asynchronous condition evaluated on success.</param>
        /// <param name="error">The error returned when the condition is false.</param>
        /// <returns>A task containing the original result if already failed or the condition is true; otherwise, a failure containing the original value and supplied error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, predicate.
        /// </exception>
        public static Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Result<T, TError> result,
            Func<T, Task<bool>> predicate,
            TError error)
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
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="predicate">The asynchronous condition evaluated on success.</param>
        /// <param name="error">The error returned when the condition is false.</param>
        /// <param name="cancellationToken">The cancellation token checked before invoking the selected delegate and passed to it.</param>
        /// <returns>A task containing the original result if already failed or the condition is true; otherwise, a failure containing the original value and supplied error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, predicate.
        /// </exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Result<T, TError> result,
            Func<T, CancellationToken, Task<bool>> predicate,
            TError error,
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
                : Result<T, TError>.Fail(value, error);
        }

        /// <summary>
        /// Runs every validator and combines returned errors using the supplied function.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="combineErrors">
        /// Combines the collected errors into one error of the same type.
        /// Called once if any validators fail, including when only one fails.
        /// </param>
        /// <param name="validators">The independent validators to execute in order.</param>
        /// <returns>
        /// The original result if already failed or no validators fail.
        /// Otherwise, a failure with the original value and combined error.
        /// </returns>
        /// <remarks>
        /// Validators receive the same original value and should not modify it.
        /// Replacement values returned by validators are ignored.
        /// Exceptions stop execution and propagate to the caller.
        /// A null validator result causes an InvalidOperationException.
        /// Arguments are checked even when the result has already failed.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, combineErrors, validators.
        /// </exception>
        /// <exception cref="ArgumentException">The validators array contains a null entry.</exception>
        /// <exception cref="InvalidOperationException">A validator returns null.</exception>
        public static Result<T, TError> ValidateAll<T, TError>(
            this Result<T, TError> result,
            Func<IReadOnlyList<TError>, TError> combineErrors,
            params Func<T, Result<T, TError>>[] validators)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (combineErrors is null)
                throw new ArgumentNullException(nameof(combineErrors));

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
            var errors = new List<TError>();

            foreach (var validate in validators)
            {
                var validation = validate(value)
                    ?? throw new InvalidOperationException(
                        "A validator returned null.");

                if (validation.IsFailure)
                    errors.Add(validation.Error);
            }

            return errors.Count == 0
                ? result
                : Result<T, TError>.Fail(
                    value,
                    combineErrors(errors.AsReadOnly()));
        }

        /// <summary>
        /// Runs asynchronous validators sequentially and combines their returned errors.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <remarks>
        /// Uses the same aggregation rules as ValidateAll.
        /// Exceptions stop execution and propagate to the caller.
        /// </remarks>
        /// <param name="result">The current result.</param>
        /// <param name="combineErrors">The function that combines collected errors into one error. Called once if any validators fail, including when only one fails.</param>
        /// <param name="validators">The independent asynchronous validators to execute sequentially in the supplied order.</param>
        /// <returns>A task containing the original result if already failed or no validators fail; otherwise, a failure containing the original value and combined error.</returns>
        /// <exception cref="ArgumentNullException">
        /// One of the following arguments is null: result, combineErrors, validators.
        /// </exception>
        /// <exception cref="ArgumentException">The validators array contains a null entry.</exception>
        /// <exception cref="InvalidOperationException">A validator returns null.</exception>
        public static Task<Result<T, TError>> ValidateAllAsync<T, TError>(
            this Result<T, TError> result,
            Func<IReadOnlyList<TError>, TError> combineErrors,
            params Func<T, Task<Result<T, TError>>>[] validators)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (combineErrors is null)
                throw new ArgumentNullException(nameof(combineErrors));

            if (validators is null)
                throw new ArgumentNullException(nameof(validators));

            var adapted =
                new Func<T, CancellationToken, Task<Result<T, TError>>>[
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
                combineErrors,
                CancellationToken.None,
                adapted);
        }

        /// <summary>
        /// Runs asynchronous validators sequentially with cancellation support
        /// and combines their returned errors.
        /// </summary>
        /// <typeparam name="T">The current value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="combineErrors">
        /// Combines the collected errors into one error of the same type.
        /// Called once if any validators fail, including when only one fails.
        /// </param>
        /// <param name="cancellationToken">
        /// Checked before each validator and passed to that validator.
        /// </param>
        /// <param name="validators">The independent validators to execute in order.</param>
        /// <returns>
        /// The original result if already failed or no validators fail.
        /// Otherwise, a failure with the original value and combined error.
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
        /// One of the following arguments is null: result, combineErrors, validators.
        /// </exception>
        /// <exception cref="ArgumentException">The validators array contains a null entry.</exception>
        /// <exception cref="InvalidOperationException">A validator returns null.</exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before an invoked delegate or reported by that delegate.</exception>
        public static async Task<Result<T, TError>> ValidateAllAsync<T, TError>(
            this Result<T, TError> result,
            Func<IReadOnlyList<TError>, TError> combineErrors,
            CancellationToken cancellationToken,
            params Func<T, CancellationToken, Task<Result<T, TError>>>[] validators)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (combineErrors is null)
                throw new ArgumentNullException(nameof(combineErrors));

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
            var errors = new List<TError>();

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
                : Result<T, TError>.Fail(
                    value,
                    combineErrors(errors.AsReadOnly()));
        }

        // Validation with lazy error creation

        /// <summary>
        /// Checks a condition on success and creates an error only when it is false.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="predicate">The condition evaluated on success.</param>
        /// <param name="errorFactory">The function called once to create an error when the condition is false.</param>
        /// <returns>
        /// The original result if already failed or the condition is true.
        /// Otherwise, a failure containing the original value and the error returned by the factory.
        /// </returns>
        /// <exception cref="ArgumentNullException">The result, predicate or error factory is null.</exception>
        /// <remarks>
        /// The error factory is not invoked for an existing failure or a true condition.
        /// Predicate and factory exceptions propagate to the caller.
        /// </remarks>
        public static Result<T, TError> Ensure<T, TError>(
            this Result<T, TError> result,
            Func<T, bool> predicate,
            Func<TError> errorFactory)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (errorFactory is null)
                throw new ArgumentNullException(nameof(errorFactory));

            if (result.IsFailure)
                return result;

            var value = result.Value;

            return predicate(value)
                ? result
                : Result<T, TError>.Fail(value, errorFactory());
        }

        /// <summary>
        /// Asynchronously checks a condition on success and creates an error only when it is false.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="predicate">The asynchronous condition evaluated on success.</param>
        /// <param name="errorFactory">The function called once to create an error when the condition is false.</param>
        /// <returns>
        /// A task containing the original result if already failed or the condition is true.
        /// Otherwise, a failure containing the original value and the error returned by the factory.
        /// </returns>
        /// <exception cref="ArgumentNullException">The result, predicate or error factory is null.</exception>
        /// <remarks>
        /// The error factory is not invoked for an existing failure or a true condition.
        /// Predicate and factory exceptions propagate to the caller.
        /// </remarks>
        public static Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Result<T, TError> result,
            Func<T, Task<bool>> predicate,
            Func<TError> errorFactory)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (errorFactory is null)
                throw new ArgumentNullException(nameof(errorFactory));

            return result.EnsureAsync(
                (value, _) => predicate(value),
                errorFactory);
        }

        /// <summary>
        /// Asynchronously checks a condition on success and creates an error only when it is false.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="result">The current result.</param>
        /// <param name="predicate">The asynchronous condition evaluated on success.</param>
        /// <param name="errorFactory">The function called once to create an error when the condition is false.</param>
        /// <param name="cancellationToken">The cancellation token checked before evaluation and passed to the predicate.</param>
        /// <returns>
        /// A task containing the original result if already failed or the condition is true.
        /// Otherwise, a failure containing the original value and the error returned by the factory.
        /// </returns>
        /// <exception cref="ArgumentNullException">The result, predicate or error factory is null.</exception>
        /// <exception cref="OperationCanceledException">Cancellation is requested before evaluation or reported by the predicate.</exception>
        /// <remarks>
        /// The error factory is not invoked for an existing failure or a true condition.
        /// Predicate and factory exceptions propagate to the caller.
        /// Existing failures are returned without checking cancellation.
        /// </remarks>
        public static async Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Result<T, TError> result,
            Func<T, CancellationToken, Task<bool>> predicate,
            Func<TError> errorFactory,
            CancellationToken cancellationToken = default)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (errorFactory is null)
                throw new ArgumentNullException(nameof(errorFactory));

            if (result.IsFailure)
                return result;

            cancellationToken.ThrowIfCancellationRequested();

            var value = result.Value;
            var isValid = await predicate(value, cancellationToken)
                .ConfigureAwait(false);

            return isValid
                ? result
                : Result<T, TError>.Fail(value, errorFactory());
        }

        /// <summary>
        /// Awaits a result and validates its value when it succeeds.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="resultTask">The task producing the result.</param>
        /// <param name="predicate">The asynchronous condition to evaluate.</param>
        /// <param name="errorFactory">
        /// Creates the error only when the condition is false.
        /// </param>
        /// <returns>
        /// A task containing the original result or a validation failure.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The result task, predicate or error factory is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The source task returns a null result.
        /// </exception>
        /// <remarks>
        /// Existing failures skip validation and error creation.
        /// Exceptions propagate to the caller.
        /// </remarks>
        public static async Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Task<Result<T, TError>> resultTask,
            Func<T, Task<bool>> predicate,
            Func<TError> errorFactory)
        {
            if (resultTask is null)
                throw new ArgumentNullException(nameof(resultTask));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (errorFactory is null)
                throw new ArgumentNullException(nameof(errorFactory));

            var result = await resultTask.ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    "The source task returned a null result.");

            if (result.IsFailure)
                return result;

            var value = result.Value;
            var isValid = await predicate(value).ConfigureAwait(false);

            return isValid
                ? result
                : Result<T, TError>.Fail(value, errorFactory());
        }

        /// <summary>
        /// Awaits a result and checks an asynchronous condition on success.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="resultTask">The task producing the result.</param>
        /// <param name="predicate">The asynchronous condition to evaluate.</param>
        /// <param name="error">The error returned when the condition is false.</param>
        /// <returns>
        /// A task containing the original result or a validation failure.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The result task or predicate is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The source task returns a null result.
        /// </exception>
        /// <remarks>
        /// Existing failures skip validation. Exceptions propagate to the caller.
        /// </remarks>
        public static Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Task<Result<T, TError>> resultTask,
            Func<T, Task<bool>> predicate,
            TError error)
        {
            if (resultTask is null)
                throw new ArgumentNullException(nameof(resultTask));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            return resultTask.EnsureAsync(
                (value, _) => predicate(value),
                error,
                CancellationToken.None);
        }

        /// <summary>
        /// Awaits a result and checks an asynchronous condition with cancellation support.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="resultTask">The task producing the result.</param>
        /// <param name="predicate">The asynchronous condition to evaluate.</param>
        /// <param name="error">The error returned when the condition is false.</param>
        /// <param name="cancellationToken">
        /// The cancellation token checked before evaluation and passed to the predicate.
        /// </param>
        /// <returns>
        /// A task containing the original result or a failure with the original value
        /// and supplied error.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The result task or predicate is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The source task returns a null result.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Cancellation is requested before evaluation or reported by the predicate.
        /// </exception>
        /// <remarks>
        /// The source task is awaited before validation. This method does not cancel it.
        /// Existing failures skip validation and cancellation checks.
        /// Exceptions propagate to the caller.
        /// </remarks>
        public static async Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Task<Result<T, TError>> resultTask,
            Func<T, CancellationToken, Task<bool>> predicate,
            TError error,
            CancellationToken cancellationToken = default)
        {
            if (resultTask is null)
                throw new ArgumentNullException(nameof(resultTask));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            var result = await resultTask.ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    "The source task returned a null result.");

            if (result.IsFailure)
                return result;

            cancellationToken.ThrowIfCancellationRequested();

            var value = result.Value;
            var isValid = await predicate(value, cancellationToken)
                .ConfigureAwait(false);

            return isValid
                ? result
                : Result<T, TError>.Fail(value, error);
        }

        /// <summary>
        /// Awaits a result and validates its value with cancellation support.
        /// Creates an error only when the condition is false.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <typeparam name="TError">The error type.</typeparam>
        /// <param name="resultTask">The task producing the result.</param>
        /// <param name="predicate">The asynchronous condition to evaluate.</param>
        /// <param name="errorFactory">
        /// Creates the error only when the condition is false.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token checked before evaluation and passed to the predicate.
        /// </param>
        /// <returns>
        /// A task containing the original result or a failure containing the original
        /// value and the error produced by the factory.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The result task, predicate or error factory is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The source task returns a null result.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Cancellation is requested before evaluation or reported by the predicate.
        /// </exception>
        /// <remarks>
        /// The source task is awaited before validation. This method does not cancel it.
        /// Existing failures skip validation, error creation and cancellation checks.
        /// Source task, predicate and factory exceptions propagate to the caller.
        /// </remarks>
        public static async Task<Result<T, TError>> EnsureAsync<T, TError>(
            this Task<Result<T, TError>> resultTask,
            Func<T, CancellationToken, Task<bool>> predicate,
            Func<TError> errorFactory,
            CancellationToken cancellationToken = default)
        {
            if (resultTask is null)
                throw new ArgumentNullException(nameof(resultTask));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (errorFactory is null)
                throw new ArgumentNullException(nameof(errorFactory));

            var result = await resultTask.ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    "The source task returned a null result.");

            if (result.IsFailure)
                return result;

            cancellationToken.ThrowIfCancellationRequested();

            var value = result.Value;
            var isValid = await predicate(value, cancellationToken)
                .ConfigureAwait(false);

            return isValid
                ? result
                : Result<T, TError>.Fail(value, errorFactory());
        }
    }
}