using Optima.Net.Exceptions;
using Optima.Net.Extensions.Result;
using Optima.Net.Result;

namespace Optima.Net.Test
{
    public class ResultTests
    {
        [Fact]
        public void Ok_ShouldCreateSuccessfulResult_WithGivenValue()
        {
            // Arrange
            var value = 42;

            // Act
            var result = Result<int>.Ok(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(value, result.Value);

            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = result.Error;
            });
        }

        [Fact]
        public void Fail_ShouldCreateFailureResult_WithGivenError()
        {
            // Arrange
            var error = "Something went wrong.";

            // Act
            var result = Result<string>.Fail("Bad", error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal("Bad", result.Value);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Ok_ShouldThrowNullValueException_WhenValueIsNull()
        {
            // Act
            var ex = Assert.Throws<NullValueException>(() => Result<string>.Ok(null!));

            // Assert
            Assert.Equal("Result<String> has no value.", ex.Message);
            //Assert.Contains("Result", ex.Message);
            //Assert.Contains("value", ex.Message);
        }

        [Fact]
        public void Fail_ShouldDefaultValueType_WhenFailed()
        {
            // Act
            var result = Result<int>.Fail(5, "bad things happened");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(5, result.Value);
            Assert.Equal("bad things happened", result.Error);
        }

        [Fact]
        public void IsFailure_ShouldBeInverseOfIsSuccess()
        {
            var okResult = Result<bool>.Ok(true);
            var failResult = Result<bool>.Fail(false, "error");

            Assert.False(okResult.IsFailure);
            Assert.True(failResult.IsFailure);
        }

        [Fact]
        public void Then_ChainsSuccessfulValidatorsAndPassesTheirValues()
        {
            var original = new Input();
            var replacement = new Input();
            var expected = Result<Input>.Ok(replacement);
            Input received = null!;

            var actual = Result<Input>.Ok(original)
                .Then(value =>
                {
                    Assert.Same(original, value);
                    return expected;
                })
                .Then(value =>
                {
                    received = value;
                    return expected;
                });

            Assert.Same(replacement, received);
            Assert.Same(expected, actual);
        }

        [Fact]
        public void Then_StopsAtFirstFailureAndPreservesThatResult()
        {
            var input = new Input();
            var failure = Result<Input>.Fail(input, "First failure");
            var calls = 0;

            var actual = Result<Input>.Ok(input)
                .Then(_ => { calls++; return failure; })
                .Then(_ => { calls++; return Result<Input>.Ok(input); });

            Assert.Equal(1, calls);
            Assert.Same(failure, actual);
        }

        [Fact]
        public void ExistingFailure_SkipsEverySynchronousValidationWithoutReadingValue()
        {
            // Use an error-only failure to exercise legacy value access guards.
            var failure = Result<Input>.Fail("Existing failure");
            var calls = 0;

            var actual = failure
                .Then(_ => { calls++; return Result<Input>.Ok(new Input()); })
                .Ensure(_ => { calls++; return true; }, "Unused")
                .ValidateAll(_ => { calls++; return Result<Input>.Ok(new Input()); });

            Assert.Equal(0, calls);
            Assert.Same(failure, actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Ensure_EvaluatesConditionAndPreservesInput(bool passes)
        {
            var input = new Input();
            var source = Result<Input>.Ok(input);
            var calls = 0;
            var actual = source.Ensure(value =>
            {
                calls++;
                Assert.Same(input, value);
                return passes;
            }, "Condition failed");

            Assert.Equal(1, calls);
            Assert.Equal(passes, actual.IsSuccess);
            Assert.Same(input, actual.Value);
            if (passes)
                Assert.Same(source, actual);
            else
                Assert.Equal("Condition failed", actual.Error);
        }

        [Fact]
        public void ValidateAll_RunsFourValidatorsAndAggregatesFailuresInOrder()
        {
            var input = new Input();
            var calls = new List<int>();
            var actual = Result<Input>.Ok(input).ValidateAll(
                _ => { calls.Add(1); return Result<Input>.Fail("Name is required."); },
                _ => { calls.Add(2); return Result<Input>.Ok(input); },
                _ => { calls.Add(3); return Result<Input>.Fail("Phone is required."); },
                _ => { calls.Add(4); return Result<Input>.Fail("Address is required."); });

            Assert.Equal(new[] { 1, 2, 3, 4 }, calls);
            Assert.True(actual.IsFailure);
            Assert.Same(input, actual.Value);
            Assert.Equal(string.Join(Environment.NewLine,
                "Name is required.", "Phone is required.", "Address is required."), actual.Error);
        }

        [Fact]
        public void ValidateAll_AllSuccessfulChecksReceiveOriginalInput()
        {
            var input = new Input();
            var source = Result<Input>.Ok(input);
            var calls = 0;
            Func<Input, Result<Input>> validate = value =>
            {
                calls++;
                Assert.Same(input, value);
                return Result<Input>.Ok(new Input());
            };

            var actual = source.ValidateAll(validate, validate);

            Assert.Equal(2, calls);
            Assert.Same(source, actual);
        }

        [Fact]
        public void ValidateAll_EmptyValidatorsReturnsOriginalResult()
        {
            var source = Result<Input>.Ok(new Input());
            Assert.Same(source, source.ValidateAll());
        }

        [Fact]
        public void ValidateAll_PreservesDuplicateErrors()
        {
            var actual = Result<Input>.Ok(new Input()).ValidateAll(
                _ => Result<Input>.Fail("Required"),
                _ => Result<Input>.Fail("Required"));

            Assert.Equal("Required" + Environment.NewLine + "Required", actual.Error);
        }

        [Fact]
        public void ValidationStages_SkipDependentChecksAfterAggregatedFailures()
        {
            var calls = 0;
            var actual = Result<Input>.Ok(new Input())
                .Ensure(_ => true, "Prerequisite failed")
                .ValidateAll(
                    _ => Result<Input>.Fail("Name"),
                    _ => Result<Input>.Fail("Email"))
                .Then(value => { calls++; return Result<Input>.Ok(value); });

            Assert.Equal(0, calls);
            Assert.Equal("Name" + Environment.NewLine + "Email", actual.Error);
        }

        [Theory]
        [InlineData("Then")]
        [InlineData("Ensure")]
        [InlineData("ValidateAll")]
        public void SynchronousValidation_PropagatesDelegateExceptions(string operation)
        {
            var source = Result<Input>.Ok(new Input());
            var expected = new InvalidOperationException("Delegate failed");
            var laterCalls = 0;
            Action execute = () =>
            {
                switch (operation)
                {
                    case "Then": source.Then(_ => throw expected); break;
                    case "Ensure": source.Ensure(_ => throw expected, "Unused"); break;
                    default:
                        source.ValidateAll(
                            _ => throw expected,
                            value => { laterCalls++; return Result<Input>.Ok(value); });
                        break;
                }
            };

            Assert.Same(expected, Assert.Throws<InvalidOperationException>(execute));
            Assert.Equal(0, laterCalls);
        }

        [Fact]
        public void SynchronousValidation_RejectsNullArguments()
        {
            Result<Input> missing = null!;
            var source = Result<Input>.Ok(new Input());

            Assert.Equal("result", Assert.Throws<ArgumentNullException>(() => missing.Then(Result<Input>.Ok)).ParamName);
            Assert.Equal("result", Assert.Throws<ArgumentNullException>(() => missing.Ensure(_ => true, "Unused")).ParamName);
            Assert.Equal("result", Assert.Throws<ArgumentNullException>(() => missing.ValidateAll()).ParamName);
            Assert.Equal("validate", Assert.Throws<ArgumentNullException>(() => source.Then(null!)).ParamName);
            Assert.Equal("predicate", Assert.Throws<ArgumentNullException>(() => source.Ensure(null!, "Unused")).ParamName);
            Assert.Equal("validators", Assert.Throws<ArgumentNullException>(() => source.ValidateAll(null!)).ParamName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateAll_RejectsNullEntriesBeforeExecutingValidators(bool alreadyFailed)
        {
            var source = alreadyFailed ? Result<Input>.Fail("Existing") : Result<Input>.Ok(new Input());
            var calls = 0;
            var error = Assert.Throws<ArgumentException>(() => source.ValidateAll(
                value => { calls++; return Result<Input>.Ok(value); }, null!));

            Assert.Equal("validators", error.ParamName);
            Assert.Equal(0, calls);
        }

        [Fact]
        public void ValidateAll_NullResultStopsRemainingValidators()
        {
            var calls = 0;
            Assert.Throws<InvalidOperationException>(() => Result<Input>.Ok(new Input()).ValidateAll(
                _ => null!,
                value => { calls++; return Result<Input>.Ok(value); }));
            Assert.Equal(0, calls);
        }

        // Each boolean test case exercises a different asynchronous overload.
        // False uses delegates without tokens. True uses cancellation-aware delegates.

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ThenAsync_AwaitsValidatorAndReturnsItsExactResult(bool withToken)
        {
            var input = new Input();
            var expected = Result<Input>.Ok(new Input());
            var actual = await RunThen(Result<Input>.Ok(input), async (value, _) =>
            {
                await Task.Yield();
                Assert.Same(input, value);
                return expected;
            }, withToken);

            Assert.Same(expected, actual);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ThenAsync_StopsAtFirstFailure(bool withToken)
        {
            var failure = Result<Input>.Fail("First failure");
            var calls = 0;
            var result = await RunThen(Result<Input>.Ok(new Input()),
                (_, __) => Task.FromResult(failure), withToken);
            result = await RunThen(result, (value, _) =>
            {
                calls++;
                return Task.FromResult(Result<Input>.Ok(value));
            }, withToken);

            Assert.Same(failure, result);
            Assert.Equal(0, calls);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public async Task EnsureAsync_AwaitsConditionAndPreservesInput(bool withToken, bool passes)
        {
            var input = new Input();
            var source = Result<Input>.Ok(input);
            var actual = await RunEnsure(source, async (value, _) =>
            {
                await Task.Yield();
                Assert.Same(input, value);
                return passes;
            }, withToken);

            Assert.Equal(passes, actual.IsSuccess);
            Assert.Same(input, actual.Value);
            if (passes)
                Assert.Same(source, actual);
            else
                Assert.Equal("Condition failed", actual.Error);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ValidateAllAsync_AwaitsSequentiallyAndAggregatesInOrder(bool withToken)
        {
            var input = new Input();
            var calls = new List<string>();
            var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pending = RunAll(Result<Input>.Ok(input), withToken, CancellationToken.None,
                async (value, _) =>
                {
                    Assert.Same(input, value);
                    calls.Add("First started");
                    await gate.Task.ConfigureAwait(false);
                    calls.Add("First finished");
                    return Result<Input>.Fail("Name");
                },
                (value, _) =>
                {
                    Assert.Same(input, value);
                    calls.Add("Second");
                    return Task.FromResult(Result<Input>.Ok(new Input()));
                },
                (value, _) =>
                {
                    Assert.Same(input, value);
                    calls.Add("Third");
                    return Task.FromResult(Result<Input>.Fail("Phone"));
                },
                (value, _) =>
                {
                    Assert.Same(input, value);
                    calls.Add("Fourth");
                    return Task.FromResult(Result<Input>.Fail("Address"));
                });

            try
            {
                Assert.False(pending.IsCompleted);
                Assert.Equal(new[] { "First started" }, calls);
            }
            finally
            {
                gate.TrySetResult(true);
            }

            var actual = await pending;
            Assert.Equal(new[] { "First started", "First finished", "Second", "Third", "Fourth" }, calls);
            Assert.True(actual.IsFailure);
            Assert.Same(input, actual.Value);
            Assert.Equal(string.Join(Environment.NewLine, "Name", "Phone", "Address"), actual.Error);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ValidateAllAsync_SuccessAndEmptyValidatorsPreserveOriginalResult(bool withToken)
        {
            var source = Result<Input>.Ok(new Input());
            var actual = await RunAll(source, withToken, CancellationToken.None,
                (_, __) => Task.FromResult(Result<Input>.Ok(new Input())),
                (_, __) => Task.FromResult(Result<Input>.Ok(new Input())));

            Assert.Same(source, actual);
            Assert.Same(source, await RunAll(source, withToken, CancellationToken.None));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AsyncValidation_ExistingFailureSkipsDelegatesAndCancellation(bool withToken)
        {
            var failure = Result<Input>.Fail("Existing failure");
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var calls = 0;
            Func<Input, CancellationToken, Task<Result<Input>>> validate = (value, _) =>
            {
                calls++;
                return Task.FromResult(Result<Input>.Ok(value));
            };

            Assert.Same(failure, await RunThen(failure, validate, withToken, cancellation.Token));
            Assert.Same(failure, await RunEnsure(failure, (_, __) => { calls++; return Task.FromResult(true); }, withToken, cancellation.Token));
            Assert.Same(failure, await RunAll(failure, withToken, cancellation.Token, validate));
            Assert.Equal(0, calls);
        }

        [Theory]
        [InlineData("Then")]
        [InlineData("Ensure")]
        [InlineData("ValidateAll")]
        public async Task AsyncValidation_PreCanceledTokenPreventsExecution(string operation)
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var calls = 0;
            var source = Result<Input>.Ok(new Input());
            Func<Input, CancellationToken, Task<Result<Input>>> validate = (value, _) =>
            {
                calls++;
                return Task.FromResult(Result<Input>.Ok(value));
            };

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                if (operation == "Then")
                    await RunThen(source, validate, true, cancellation.Token);
                else if (operation == "Ensure")
                    await RunEnsure(source, (_, __) => { calls++; return Task.FromResult(true); }, true, cancellation.Token);
                else
                    await RunAll(source, true, cancellation.Token, validate);
            });
            Assert.Equal(0, calls);
        }

        [Fact]
        public async Task AsyncValidation_ForwardsCancellationTokenToDelegates()
        {
            using var cancellation = new CancellationTokenSource();
            var source = Result<Input>.Ok(new Input());
            Func<Input, CancellationToken, Task<Result<Input>>> validate = (value, token) =>
            {
                Assert.Equal(cancellation.Token, token);
                return Task.FromResult(Result<Input>.Ok(value));
            };

            await RunThen(source, validate, true, cancellation.Token);
            await RunEnsure(source, (_, token) =>
            {
                Assert.Equal(cancellation.Token, token);
                return Task.FromResult(true);
            }, true, cancellation.Token);
            await RunAll(source, true, cancellation.Token, validate, validate);
        }

        [Fact]
        public async Task ValidateAllAsync_CancellationBetweenValidatorsStopsInsteadOfReturningPartialErrors()
        {
            using var cancellation = new CancellationTokenSource();
            var laterCalls = 0;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await RunAll(Result<Input>.Ok(new Input()), true, cancellation.Token,
                    (_, __) =>
                    {
                        cancellation.Cancel();
                        return Task.FromResult(Result<Input>.Fail("Partial error"));
                    },
                    (value, _) => { laterCalls++; return Task.FromResult(Result<Input>.Ok(value)); }));

            Assert.Equal(0, laterCalls);
        }

        [Fact]
        public async Task ValidateAllAsync_EmptyValidatorsDoNotCheckCancellation()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var source = Result<Input>.Ok(new Input());
            Assert.Same(source, await RunAll(source, true, cancellation.Token));
        }

        [Theory]
        [InlineData(false, "Then")]
        [InlineData(true, "Then")]
        [InlineData(false, "Ensure")]
        [InlineData(true, "Ensure")]
        [InlineData(false, "ValidateAll")]
        [InlineData(true, "ValidateAll")]
        public async Task AsyncValidation_PropagatesFaultedTasks(bool withToken, string operation)
        {
            var expected = new InvalidOperationException("Async delegate failed");
            var source = Result<Input>.Ok(new Input());
            var laterCalls = 0;
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                if (operation == "Then")
                    await RunThen(source, (_, __) => Task.FromException<Result<Input>>(expected), withToken);
                else if (operation == "Ensure")
                    await RunEnsure(source, (_, __) => Task.FromException<bool>(expected), withToken);
                else
                    await RunAll(source, withToken, CancellationToken.None,
                        (_, __) => Task.FromException<Result<Input>>(expected),
                        (value, _) => { laterCalls++; return Task.FromResult(Result<Input>.Ok(value)); });
            });

            Assert.Same(expected, actual);
            Assert.Equal(0, laterCalls);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AsyncValidation_RejectsNullResults(bool withToken)
        {
            var source = Result<Input>.Ok(new Input());
            Func<Input, CancellationToken, Task<Result<Input>>> validate = (_, __) => Task.FromResult<Result<Input>>(null!);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await RunThen(source, validate, withToken));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await RunAll(source, withToken, CancellationToken.None, validate));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AsyncValidation_RejectsNullArguments(bool withToken)
        {
            Result<Input> missing = null!;
            var source = Result<Input>.Ok(new Input());
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await RunThen(missing, (_, __) => Task.FromResult(source), withToken));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await RunEnsure(missing, (_, __) => Task.FromResult(true), withToken));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await RunAll(missing, withToken, CancellationToken.None));

            // Call actual overloads directly so adapters cannot hide null arguments.
            if (withToken)
            {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.ThenAsync((Func<Input, CancellationToken, Task<Result<Input>>>)null!));
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.EnsureAsync((Func<Input, CancellationToken, Task<bool>>)null!, "Unused"));
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.ValidateAllAsync(CancellationToken.None, null!));
            }
            else
            {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.ThenAsync((Func<Input, Task<Result<Input>>>)null!));
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.EnsureAsync((Func<Input, Task<bool>>)null!, "Unused"));
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.ValidateAllAsync((Func<Input, Task<Result<Input>>>[])null!));
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ValidateAllAsync_RejectsNullEntriesBeforeExecution(bool withToken, bool alreadyFailed)
        {
            var source = alreadyFailed ? Result<Input>.Fail("Existing") : Result<Input>.Ok(new Input());
            var calls = 0;
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                if (withToken)
                    await source.ValidateAllAsync(CancellationToken.None,
                        (value, _) => { calls++; return Task.FromResult(Result<Input>.Ok(value)); }, null!);
                else
                    await source.ValidateAllAsync(
                        value => { calls++; return Task.FromResult(Result<Input>.Ok(value)); }, null!);
            });
            Assert.Equal(0, calls);
        }

        private static Task<Result<Input>> RunThen(
            Result<Input> result,
            Func<Input, CancellationToken, Task<Result<Input>>> validate,
            bool withToken,
            CancellationToken token = default) =>
            withToken
                ? result.ThenAsync(validate, token)
                : result.ThenAsync(value => validate(value, CancellationToken.None));

        private static Task<Result<Input>> RunEnsure(
            Result<Input> result,
            Func<Input, CancellationToken, Task<bool>> predicate,
            bool withToken,
            CancellationToken token = default) =>
            withToken
                ? result.EnsureAsync(predicate, "Condition failed", token)
                : result.EnsureAsync(value => predicate(value, CancellationToken.None), "Condition failed");

        private static Task<Result<Input>> RunAll(
            Result<Input> result,
            bool withToken,
            CancellationToken token,
            params Func<Input, CancellationToken, Task<Result<Input>>>[] validators)
        {
            if (withToken)
                return result.ValidateAllAsync(token, validators);

            var adapted = new Func<Input, Task<Result<Input>>>[validators.Length];
            for (var i = 0; i < validators.Length; i++)
            {
                var validator = validators[i];
                adapted[i] = value => validator(value, CancellationToken.None);
            }
            return result.ValidateAllAsync(adapted);
        }

        private sealed class Input
        {
        }
    }
}
