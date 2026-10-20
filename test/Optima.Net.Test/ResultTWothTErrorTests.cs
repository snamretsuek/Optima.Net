using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Optima.Net.Extensions.Result;
using Optima.Net.Result;

namespace Optima.Net.Test
{
    public class ResultTWithTErrorTests
    {
        private record TestError(string Message);

        private static TestError CombineErrors(IReadOnlyList<TestError> errors)
        {
            var messages = new List<string>();
            foreach (var error in errors)
                messages.Add(error.Message);
            return new TestError(string.Join(Environment.NewLine, messages));
        }

        private sealed class Input
        {
        }

        [Fact]
        public void Then_ChainsSuccessfulValidatorsAndPassesTheirValues()
        {
            var original = new Input();
            var replacement = new Input();
            var expected = Result<Input, TestError>.Ok(replacement);
            Input received = null!;

            var actual = Result<Input, TestError>.Ok(original)
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
            var failure = Result<Input, TestError>.Fail(input, new TestError("First failure"));
            var calls = 0;

            var actual = Result<Input, TestError>.Ok(input)
                .Then(_ => { calls++; return failure; })
                .Then(_ => { calls++; return Result<Input, TestError>.Ok(input); });

            Assert.Equal(1, calls);
            Assert.Same(failure, actual);
        }

        [Fact]
        public void ExistingFailure_SkipsEverySynchronousValidationWithoutReadingValue()
        {
            // Use an error-only failure to exercise legacy value access guards.
            var failure = Result<Input, TestError>.Fail(new TestError("Existing failure"));
            var calls = 0;

            var actual = failure
                .Then(_ => { calls++; return Result<Input, TestError>.Ok(new Input()); })
                .Ensure(_ => { calls++; return true; }, new TestError("Unused"))
                .ValidateAll(CombineErrors, _ => { calls++; return Result<Input, TestError>.Ok(new Input()); });

            Assert.Equal(0, calls);
            Assert.Same(failure, actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Ensure_EvaluatesConditionAndPreservesInput(bool passes)
        {
            var input = new Input();
            var source = Result<Input, TestError>.Ok(input);
            var calls = 0;
            var actual = source.Ensure(value =>
            {
                calls++;
                Assert.Same(input, value);
                return passes;
            }, new TestError("Condition failed"));

            Assert.Equal(1, calls);
            Assert.Equal(passes, actual.IsSuccess);
            Assert.Same(input, actual.Value);
            if (passes)
                Assert.Same(source, actual);
            else
                Assert.Equal(new TestError("Condition failed"), actual.Error);
        }

        [Fact]
        public void ValidateAll_RunsFourValidatorsAndAggregatesFailuresInOrder()
        {
            var input = new Input();
            var calls = new List<int>();
            var actual = Result<Input, TestError>.Ok(input).ValidateAll(CombineErrors,
                _ => { calls.Add(1); return Result<Input, TestError>.Fail(new TestError("Name is required.")); },
                _ => { calls.Add(2); return Result<Input, TestError>.Ok(input); },
                _ => { calls.Add(3); return Result<Input, TestError>.Fail(new TestError("Phone is required.")); },
                _ => { calls.Add(4); return Result<Input, TestError>.Fail(new TestError("Address is required.")); });

            Assert.Equal(new[] { 1, 2, 3, 4 }, calls);
            Assert.True(actual.IsFailure);
            Assert.Same(input, actual.Value);
            Assert.Equal(string.Join(Environment.NewLine,
                "Name is required.", "Phone is required.", "Address is required."), actual.Error.Message);
        }

        [Fact]
        public void ValidateAll_AllSuccessfulChecksReceiveOriginalInput()
        {
            var input = new Input();
            var source = Result<Input, TestError>.Ok(input);
            var calls = 0;
            Func<Input, Result<Input, TestError>> validate = value =>
            {
                calls++;
                Assert.Same(input, value);
                return Result<Input, TestError>.Ok(new Input());
            };

            var actual = source.ValidateAll(CombineErrors, validate, validate);

            Assert.Equal(2, calls);
            Assert.Same(source, actual);
        }

        [Fact]
        public void ValidateAll_EmptyValidatorsReturnsOriginalResult()
        {
            var source = Result<Input, TestError>.Ok(new Input());
            Assert.Same(source, source.ValidateAll(CombineErrors));
        }

        [Fact]
        public void ValidateAll_PreservesDuplicateErrors()
        {
            var actual = Result<Input, TestError>.Ok(new Input()).ValidateAll(CombineErrors,
                _ => Result<Input, TestError>.Fail(new TestError("Required")),
                _ => Result<Input, TestError>.Fail(new TestError("Required")));

            Assert.Equal("Required" + Environment.NewLine + "Required", actual.Error.Message);
        }

        [Fact]
        public void ValidationStages_SkipDependentChecksAfterAggregatedFailures()
        {
            var calls = 0;
            var actual = Result<Input, TestError>.Ok(new Input())
                .Ensure(_ => true, new TestError("Prerequisite failed"))
                .ValidateAll(CombineErrors,
                    _ => Result<Input, TestError>.Fail(new TestError("Name")),
                    _ => Result<Input, TestError>.Fail(new TestError("Email")))
                .Then(value => { calls++; return Result<Input, TestError>.Ok(value); });

            Assert.Equal(0, calls);
            Assert.Equal("Name" + Environment.NewLine + "Email", actual.Error.Message);
        }

        [Theory]
        [InlineData("Then")]
        [InlineData("Ensure")]
        [InlineData("ValidateAll")]
        public void SynchronousValidation_PropagatesDelegateExceptions(string operation)
        {
            var source = Result<Input, TestError>.Ok(new Input());
            var expected = new InvalidOperationException("Delegate failed");
            var laterCalls = 0;
            Action execute = () =>
            {
                switch (operation)
                {
                    case "Then": source.Then(_ => throw expected); break;
                    case "Ensure": source.Ensure(_ => throw expected, new TestError("Unused")); break;
                    default:
                        source.ValidateAll(CombineErrors,
                            _ => throw expected,
                            value => { laterCalls++; return Result<Input, TestError>.Ok(value); });
                        break;
                }
            };

            Assert.Same(expected, Assert.Throws<InvalidOperationException>(execute));
            Assert.Equal(0, laterCalls);
        }

        [Fact]
        public void SynchronousValidation_RejectsNullArguments()
        {
            Result<Input, TestError> missing = null!;
            var source = Result<Input, TestError>.Ok(new Input());

            Assert.Equal("result", Assert.Throws<ArgumentNullException>(() => missing.Then(Result<Input, TestError>.Ok)).ParamName);
            Assert.Equal("result", Assert.Throws<ArgumentNullException>(() => missing.Ensure(_ => true, new TestError("Unused"))).ParamName);
            Assert.Equal("result", Assert.Throws<ArgumentNullException>(() => missing.ValidateAll(CombineErrors)).ParamName);
            Assert.Equal("validate", Assert.Throws<ArgumentNullException>(() => source.Then(null!)).ParamName);
            Assert.Equal("predicate", Assert.Throws<ArgumentNullException>(() => source.Ensure(null!, new TestError("Unused"))).ParamName);
            Assert.Equal("validators", Assert.Throws<ArgumentNullException>(() => source.ValidateAll(CombineErrors, null!)).ParamName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateAll_RejectsNullEntriesBeforeExecutingValidators(bool alreadyFailed)
        {
            var source = alreadyFailed ? Result<Input, TestError>.Fail(new TestError("Existing")) : Result<Input, TestError>.Ok(new Input());
            var calls = 0;
            var error = Assert.Throws<ArgumentException>(() => source.ValidateAll(CombineErrors,
                value => { calls++; return Result<Input, TestError>.Ok(value); }, null!));

            Assert.Equal("validators", error.ParamName);
            Assert.Equal(0, calls);
        }

        [Fact]
        public void ValidateAll_NullResultStopsRemainingValidators()
        {
            var calls = 0;
            Assert.Throws<InvalidOperationException>(() => Result<Input, TestError>.Ok(new Input()).ValidateAll(CombineErrors,
                _ => null!,
                value => { calls++; return Result<Input, TestError>.Ok(value); }));
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
            var expected = Result<Input, TestError>.Ok(new Input());
            var actual = await RunThen(Result<Input, TestError>.Ok(input), async (value, _) =>
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
            var failure = Result<Input, TestError>.Fail(new TestError("First failure"));
            var calls = 0;
            var result = await RunThen(Result<Input, TestError>.Ok(new Input()),
                (_, __) => Task.FromResult(failure), withToken);
            result = await RunThen(result, (value, _) =>
            {
                calls++;
                return Task.FromResult(Result<Input, TestError>.Ok(value));
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
            var source = Result<Input, TestError>.Ok(input);
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
                Assert.Equal(new TestError("Condition failed"), actual.Error);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ValidateAllAsync_AwaitsSequentiallyAndAggregatesInOrder(bool withToken)
        {
            var input = new Input();
            var calls = new List<string>();
            var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pending = RunAll(Result<Input, TestError>.Ok(input), withToken, CancellationToken.None,
                async (value, _) =>
                {
                    Assert.Same(input, value);
                    calls.Add("First started");
                    await gate.Task.ConfigureAwait(false);
                    calls.Add("First finished");
                    return Result<Input, TestError>.Fail(new TestError("Name"));
                },
                (value, _) =>
                {
                    Assert.Same(input, value);
                    calls.Add("Second");
                    return Task.FromResult(Result<Input, TestError>.Ok(new Input()));
                },
                (value, _) =>
                {
                    Assert.Same(input, value);
                    calls.Add("Third");
                    return Task.FromResult(Result<Input, TestError>.Fail(new TestError("Phone")));
                },
                (value, _) =>
                {
                    Assert.Same(input, value);
                    calls.Add("Fourth");
                    return Task.FromResult(Result<Input, TestError>.Fail(new TestError("Address")));
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
            Assert.Equal(string.Join(Environment.NewLine, "Name", "Phone", "Address"), actual.Error.Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ValidateAllAsync_SuccessAndEmptyValidatorsPreserveOriginalResult(bool withToken)
        {
            var source = Result<Input, TestError>.Ok(new Input());
            var actual = await RunAll(source, withToken, CancellationToken.None,
                (_, __) => Task.FromResult(Result<Input, TestError>.Ok(new Input())),
                (_, __) => Task.FromResult(Result<Input, TestError>.Ok(new Input())));

            Assert.Same(source, actual);
            Assert.Same(source, await RunAll(source, withToken, CancellationToken.None));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AsyncValidation_ExistingFailureSkipsDelegatesAndCancellation(bool withToken)
        {
            var failure = Result<Input, TestError>.Fail(new TestError("Existing failure"));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var calls = 0;
            Func<Input, CancellationToken, Task<Result<Input, TestError>>> validate = (value, _) =>
            {
                calls++;
                return Task.FromResult(Result<Input, TestError>.Ok(value));
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
            var source = Result<Input, TestError>.Ok(new Input());
            Func<Input, CancellationToken, Task<Result<Input, TestError>>> validate = (value, _) =>
            {
                calls++;
                return Task.FromResult(Result<Input, TestError>.Ok(value));
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
            var source = Result<Input, TestError>.Ok(new Input());
            Func<Input, CancellationToken, Task<Result<Input, TestError>>> validate = (value, token) =>
            {
                Assert.Equal(cancellation.Token, token);
                return Task.FromResult(Result<Input, TestError>.Ok(value));
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
                await RunAll(Result<Input, TestError>.Ok(new Input()), true, cancellation.Token,
                    (_, __) =>
                    {
                        cancellation.Cancel();
                        return Task.FromResult(Result<Input, TestError>.Fail(new TestError("Partial error")));
                    },
                    (value, _) => { laterCalls++; return Task.FromResult(Result<Input, TestError>.Ok(value)); }));

            Assert.Equal(0, laterCalls);
        }

        [Fact]
        public async Task ValidateAllAsync_EmptyValidatorsDoNotCheckCancellation()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var source = Result<Input, TestError>.Ok(new Input());
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
            var source = Result<Input, TestError>.Ok(new Input());
            var laterCalls = 0;
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                if (operation == "Then")
                    await RunThen(source, (_, __) => Task.FromException<Result<Input, TestError>>(expected), withToken);
                else if (operation == "Ensure")
                    await RunEnsure(source, (_, __) => Task.FromException<bool>(expected), withToken);
                else
                    await RunAll(source, withToken, CancellationToken.None,
                        (_, __) => Task.FromException<Result<Input, TestError>>(expected),
                        (value, _) => { laterCalls++; return Task.FromResult(Result<Input, TestError>.Ok(value)); });
            });

            Assert.Same(expected, actual);
            Assert.Equal(0, laterCalls);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AsyncValidation_RejectsNullResults(bool withToken)
        {
            var source = Result<Input, TestError>.Ok(new Input());
            Func<Input, CancellationToken, Task<Result<Input, TestError>>> validate = (_, __) => Task.FromResult<Result<Input, TestError>>(null!);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await RunThen(source, validate, withToken));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await RunAll(source, withToken, CancellationToken.None, validate));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AsyncValidation_RejectsNullArguments(bool withToken)
        {
            Result<Input, TestError> missing = null!;
            var source = Result<Input, TestError>.Ok(new Input());
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await RunThen(missing, (_, __) => Task.FromResult(source), withToken));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await RunEnsure(missing, (_, __) => Task.FromResult(true), withToken));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await RunAll(missing, withToken, CancellationToken.None));

            // Call actual overloads directly so adapters cannot hide null arguments.
            if (withToken)
            {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.ThenAsync((Func<Input, CancellationToken, Task<Result<Input, TestError>>>)null!));
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.EnsureAsync((Func<Input, CancellationToken, Task<bool>>)null!, new TestError("Unused")));
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.ValidateAllAsync(CombineErrors, CancellationToken.None, null!));
            }
            else
            {
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.ThenAsync((Func<Input, Task<Result<Input, TestError>>>)null!));
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.EnsureAsync((Func<Input, Task<bool>>)null!, new TestError("Unused")));
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.ValidateAllAsync(CombineErrors, (Func<Input, Task<Result<Input, TestError>>>[])null!));
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ValidateAllAsync_RejectsNullEntriesBeforeExecution(bool withToken, bool alreadyFailed)
        {
            var source = alreadyFailed ? Result<Input, TestError>.Fail(new TestError("Existing")) : Result<Input, TestError>.Ok(new Input());
            var calls = 0;
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                if (withToken)
                    await source.ValidateAllAsync(CombineErrors, CancellationToken.None,
                        (value, _) => { calls++; return Task.FromResult(Result<Input, TestError>.Ok(value)); }, null!);
                else
                    await source.ValidateAllAsync(CombineErrors,
                        value => { calls++; return Task.FromResult(Result<Input, TestError>.Ok(value)); }, null!);
            });
            Assert.Equal(0, calls);
        }

        [Fact]
        public void Ok_ShouldCreateSuccess()
        {
            var result = Result<int, TestError>.Ok(42);
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void Fail_ShouldCreateFailure()
        {
            var error = new TestError("Boom");
            var result = Result<int, TestError>.Fail(5, error);
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Same(error, result.Error);
            Assert.Equal(5, result.Value);
        }

        [Fact]
        public void Map_ShouldTransformValue_WhenSuccess()
        {
            var result = Result<int, TestError>.Ok(2).Map(x => x * 2);
            Assert.True(result.IsSuccess);
            Assert.Equal(4, result.Value);
        }

        [Fact]
        public void Map_ShouldNotExecute_WhenFailure()
        {
            var error = new TestError("Failed");
            var calls = 0;
            var result = Result<int, TestError>.Fail(5, error)
                .Map(x => { calls++; return x * 2; });
            Assert.True(result.IsFailure);
            Assert.Same(error, result.Error);
            Assert.Equal(0, calls);
        }

        [Fact]
        public void Bind_ShouldChain_WhenSuccess()
        {
            var expected = Result<string, TestError>.Ok("Value:10");
            var result = Result<int, TestError>.Ok(10).Bind(x =>
            {
                Assert.Equal(10, x);
                return expected;
            });
            Assert.Same(expected, result);
        }

        [Fact]
        public void Bind_ShouldShortCircuit_WhenFailure()
        {
            var error = new TestError("Oops");
            var calls = 0;
            var result = Result<int, TestError>.Fail(5, error).Bind(x =>
            {
                calls++;
                return Result<string, TestError>.Ok("ShouldNotRun");
            });
            Assert.True(result.IsFailure);
            Assert.Same(error, result.Error);
            Assert.Equal(0, calls);
        }

        [Fact]
        public void Match_ShouldReturnSuccessValue()
        {
            var value = Result<int, TestError>.Ok(5).Match(x => x * 2, _ => throw new Exception("Wrong branch"));
            Assert.Equal(10, value);
        }

        [Fact]
        public void Match_ShouldReturnFailureValue()
        {
            var value = Result<int, TestError>.Fail(5, new TestError("Fail"))
                .Match(_ => throw new Exception("Wrong branch"), _ => -1);
            Assert.Equal(-1, value);
        }

        [Fact]
        public void Ensure_ShouldPass_WhenPredicateTrue()
        {
            var result = Result<int, TestError>.Ok(10)
                .Ensure(x => x > 5, () => new TestError("Too small"));
            Assert.True(result.IsSuccess);
            Assert.Equal(10, result.Value);
        }

        [Fact]
        public void Ensure_ShouldFail_WhenPredicateFalse()
        {
            var result = Result<int, TestError>.Ok(2)
                .Ensure(x => x > 5, () => new TestError("Too small"));
            Assert.True(result.IsFailure);
            Assert.Equal("Too small", result.Error.Message);
        }

        [Fact]
        public async Task EnsureAsync_ShouldPass_WhenPredicateTrue()
        {
            var result = await Result<int, TestError>.Ok(9)
                .EnsureAsync(async x => { await Task.Yield(); return x == 9; }, () => new TestError("Not nine"));
            Assert.True(result.IsSuccess);
            Assert.Equal(9, result.Value);
        }

        [Fact]
        public async Task EnsureAsync_ShouldFail_WhenPredicateFalse()
        {
            var result = await Result<int, TestError>.Ok(3)
                .EnsureAsync(async x => { await Task.Yield(); return x == 9; }, () => new TestError("Nope"));
            Assert.True(result.IsFailure);
            Assert.Equal("Nope", result.Error.Message);
        }

        private static async Task<Result<int, TestError>> GetNumberAsync(int value)
        {
            await Task.Yield();
            return Result<int, TestError>.Ok(value);
        }

        [Fact]
        public async Task EnsureAsync_TaskPipeline_ShouldPass()
        {
            var result = await GetNumberAsync(8)
                .EnsureAsync(x => Task.FromResult(x == 8), () => new TestError("Wrong number"));
            Assert.True(result.IsSuccess);
            Assert.Equal(8, result.Value);
        }

        [Fact]
        public async Task EnsureAsync_TaskPipeline_ShouldFail()
        {
            var result = await GetNumberAsync(8)
                .EnsureAsync(x => Task.FromResult(x == 9), () => new TestError("Wrong number"));
            Assert.True(result.IsFailure);
            Assert.Equal("Wrong number", result.Error.Message);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Ensure_FactoryIsLazy(bool existingFailure, bool passes)
        {
            var source = existingFailure ? Result<int, TestError>.Fail(new TestError("Existing")) : Result<int, TestError>.Ok(8);
            var predicateCalls = 0;
            var factoryCalls = 0;
            var expected = new TestError("New failure");
            var result = source.Ensure(_ => { predicateCalls++; return passes; }, () => { factoryCalls++; return expected; });
            Assert.Equal(existingFailure ? 0 : 1, predicateCalls);
            Assert.Equal(!existingFailure && !passes ? 1 : 0, factoryCalls);
            if (existingFailure || passes)
                Assert.Same(source, result);
            else
            {
                Assert.Same(expected, result.Error);
                Assert.Equal(8, result.Value);
            }
        }

        // Covers both async factory overloads, directly and through a task pipeline.
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public async Task EnsureAsync_FactoryIsLazy(bool taskReceiver, bool withToken, bool passes)
        {
            using var cancellation = new CancellationTokenSource();
            var source = Result<int, TestError>.Ok(8);
            var factoryCalls = 0;
            var expected = new TestError("Failure");
            Func<TestError> factory = () => { factoryCalls++; return expected; };
            Func<int, CancellationToken, Task<bool>> predicate = async (value, token) =>
            {
                await Task.Yield();
                Assert.Equal(8, value);
                Assert.Equal(withToken ? cancellation.Token : CancellationToken.None, token);
                return passes;
            };
            Result<int, TestError> result;
            if (taskReceiver)
                result = withToken
                    ? await Task.FromResult(source).EnsureAsync(predicate, factory, cancellation.Token)
                    : await Task.FromResult(source).EnsureAsync(x => predicate(x, CancellationToken.None), factory);
            else
                result = withToken
                    ? await source.EnsureAsync(predicate, factory, cancellation.Token)
                    : await source.EnsureAsync(x => predicate(x, CancellationToken.None), factory);

            Assert.Equal(passes ? 0 : 1, factoryCalls);
            if (passes) Assert.Same(source, result);
            else { Assert.Same(expected, result.Error); Assert.Equal(8, result.Value); }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task EnsureAsync_TaskPipelineSupportsErrorValues(bool withToken)
        {
            var error = new TestError("Error value");
            var task = GetNumberAsync(8);
            var result = withToken
                ? await task.EnsureAsync((_, __) => Task.FromResult(false), error, CancellationToken.None)
                : await task.EnsureAsync(_ => Task.FromResult(false), error);
            Assert.Same(error, result.Error);
            Assert.Equal(8, result.Value);
        }

        [Fact]
        public async Task EnsureAsync_TaskPipelineAwaitsSourceBeforeValidation()
        {
            var gate = new TaskCompletionSource<Result<int, TestError>>(TaskCreationOptions.RunContinuationsAsynchronously);
            var calls = 0;
            var source = Result<int, TestError>.Ok(8);
            var pending = gate.Task.EnsureAsync(_ => { calls++; return Task.FromResult(true); }, () => new TestError("Unused"));
            try { Assert.False(pending.IsCompleted); Assert.Equal(0, calls); }
            finally { gate.TrySetResult(source); }
            Assert.Same(source, await pending);
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task EnsureAsync_TaskPipelinePropagatesSourceFailureAndExceptions()
        {
            var failure = Result<int, TestError>.Fail(new TestError("Existing"));
            var calls = 0;
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var result = await Task.FromResult(failure).EnsureAsync(
                (_, __) => { calls++; return Task.FromResult(true); },
                () => { calls++; return new TestError("Unused"); }, cancellation.Token);
            Assert.Same(failure, result);
            Assert.Equal(0, calls);
            var expected = new InvalidOperationException("Source fault");
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await Task.FromException<Result<int, TestError>>(expected)
                    .EnsureAsync(_ => Task.FromResult(true), () => new TestError("Unused")));
            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task Ensure_FactoryGuardsAndExceptionsArePreserved()
        {
            var source = Result<int, TestError>.Ok(8);
            var expected = new InvalidOperationException("Factory failed");
            Assert.Throws<ArgumentNullException>(() => source.Ensure(_ => true, (Func<TestError>)null!));
            Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => source.Ensure(_ => false, () => throw expected)));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.EnsureAsync(_ => Task.FromResult(true), (Func<TestError>)null!));
            Assert.Same(expected, await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await source.EnsureAsync(_ => Task.FromResult(false), () => throw expected)));
            Task<Result<int, TestError>> missing = null!;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await missing.EnsureAsync(_ => Task.FromResult(true), new TestError("Unused")));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await Task.FromResult<Result<int, TestError>>(null!)
                .EnsureAsync(_ => Task.FromResult(true), new TestError("Unused")));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ValidateAll_CombinerRunsExactlyOnceWhenNeeded(int failures)
        {
            var source = Result<int, TestError>.Ok(8);
            var error = new TestError("Combined");
            var calls = 0;
            Func<IReadOnlyList<TestError>, TestError> combine = errors =>
            {
                calls++;
                Assert.Equal(failures, errors.Count);
                return error;
            };
            Func<int, Result<int, TestError>> first = value => failures > 0 ? Result<int, TestError>.Fail(new TestError("First")) : Result<int, TestError>.Ok(value);
            Func<int, Result<int, TestError>> second = value => failures > 1 ? Result<int, TestError>.Fail(new TestError("Second")) : Result<int, TestError>.Ok(value);
            var result = source.ValidateAll(combine, first, second);
            Assert.Equal(failures == 0 ? 0 : 1, calls);
            if (failures == 0) Assert.Same(source, result); else Assert.Same(error, result.Error);
            calls = 0;
            result = await source.ValidateAllAsync(combine, value => Task.FromResult(first(value)), value => Task.FromResult(second(value)));
            Assert.Equal(failures == 0 ? 0 : 1, calls);
            if (failures == 0) Assert.Same(source, result); else Assert.Same(error, result.Error);
        }

        [Fact]
        public async Task ValidateAll_CombinerGuardsAndExceptionsArePreserved()
        {
            var source = Result<int, TestError>.Ok(8);
            Assert.Throws<ArgumentNullException>(() => source.ValidateAll(null!));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await source.ValidateAllAsync(null!));
            var expected = new InvalidOperationException("Combiner failed");
            Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => source.ValidateAll(
                _ => throw expected, _ => Result<int, TestError>.Fail(new TestError("Failure")))));
            Assert.Same(expected, await Assert.ThrowsAsync<InvalidOperationException>(async () => await source.ValidateAllAsync(
                _ => throw expected, _ => Task.FromResult(Result<int, TestError>.Fail(new TestError("Failure"))))));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task AsyncOperations_SelectCorrectBranch(bool failure, bool withToken)
        {
            using var cancellation = new CancellationTokenSource();
            var error = new TestError("Failure");
            var source = failure ? Result<int, TestError>.Fail(8, error) : Result<int, TestError>.Ok(8);
            var successCalls = 0;
            Func<int, CancellationToken, Task<int>> map = async (value, token) =>
            {
                await Task.Yield();
                Assert.Equal(withToken ? cancellation.Token : CancellationToken.None, token);
                successCalls++;
                return value * 2;
            };
            var mapped = withToken ? await source.MapAsync(map, cancellation.Token) : await source.MapAsync(x => map(x, CancellationToken.None));
            Assert.Equal(failure ? 0 : 1, successCalls);
            if (failure) Assert.Same(error, mapped.Error); else Assert.Equal(16, mapped.Value);

            successCalls = 0;
            Func<int, CancellationToken, Task<Result<string, TestError>>> bind = async (value, token) =>
            {
                await map(value, token);
                return Result<string, TestError>.Ok("Bound");
            };
            var bound = withToken ? await source.BindAsync(bind, cancellation.Token) : await source.BindAsync(x => bind(x, CancellationToken.None));
            Assert.Equal(failure ? 0 : 1, successCalls);
            if (failure) Assert.Same(error, bound.Error); else Assert.Equal("Bound", bound.Value);

            successCalls = 0;
            Func<int, CancellationToken, Task> tap = async (value, token) => { await map(value, token); };
            var tapped = withToken ? await source.TapAsync(tap, cancellation.Token) : await source.TapAsync(x => tap(x, CancellationToken.None));
            Assert.Same(source, tapped);
            Assert.Equal(failure ? 0 : 1, successCalls);

            var failureCalls = 0;
            Func<TestError, CancellationToken, Task> onFailure = async (value, token) =>
            {
                await Task.Yield();
                Assert.Same(error, value);
                Assert.Equal(withToken ? cancellation.Token : CancellationToken.None, token);
                failureCalls++;
            };
            var handled = withToken ? await source.OnFailureAsync(onFailure, cancellation.Token) : await source.OnFailureAsync(x => onFailure(x, CancellationToken.None));
            Assert.Same(source, handled);
            Assert.Equal(failure ? 1 : 0, failureCalls);

            successCalls = 0;
            failureCalls = 0;
            Func<TestError, CancellationToken, Task<int>> matchFailure = async (value, token) => { await onFailure(value, token); return -1; };
            var matched = withToken ? await source.MatchAsync(map, matchFailure, cancellation.Token)
                : await source.MatchAsync(x => map(x, CancellationToken.None), e => matchFailure(e, CancellationToken.None));
            Assert.Equal(failure ? -1 : 16, matched);
            Assert.Equal(failure ? 0 : 1, successCalls);
            Assert.Equal(failure ? 1 : 0, failureCalls);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TapAndOnFailure_ExecuteOnlyMatchingActions(bool failure)
        {
            var error = new TestError("Failure");
            var source = failure ? Result<int, TestError>.Fail(8, error) : Result<int, TestError>.Ok(8);
            var successCalls = 0;
            var failureCalls = 0;
            var actual = source.Tap(value => { successCalls++; Assert.Equal(8, value); })
                .OnFailure(value => { failureCalls++; Assert.Same(error, value); });
            Assert.Same(source, actual);
            Assert.Equal(failure ? 0 : 1, successCalls);
            Assert.Equal(failure ? 1 : 0, failureCalls);
        }

        private static Task<Result<Input, TestError>> RunThen(
            Result<Input, TestError> result,
            Func<Input, CancellationToken, Task<Result<Input, TestError>>> validate,
            bool withToken,
            CancellationToken token = default) =>
            withToken
                ? result.ThenAsync(validate, token)
                : result.ThenAsync(value => validate(value, CancellationToken.None));

        private static Task<Result<Input, TestError>> RunEnsure(
            Result<Input, TestError> result,
            Func<Input, CancellationToken, Task<bool>> predicate,
            bool withToken,
            CancellationToken token = default) =>
            withToken
                ? result.EnsureAsync(predicate, new TestError("Condition failed"), token)
                : result.EnsureAsync(value => predicate(value, CancellationToken.None), new TestError("Condition failed"));

        private static Task<Result<Input, TestError>> RunAll(
            Result<Input, TestError> result,
            bool withToken,
            CancellationToken token,
            params Func<Input, CancellationToken, Task<Result<Input, TestError>>>[] validators)
        {
            if (withToken)
                return result.ValidateAllAsync(CombineErrors, token, validators);

            var adapted = new Func<Input, Task<Result<Input, TestError>>>[validators.Length];
            for (var i = 0; i < validators.Length; i++)
            {
                var validator = validators[i];
                adapted[i] = value => validator(value, CancellationToken.None);
            }
            return result.ValidateAllAsync(CombineErrors, adapted);
        }
    }
}
