using Optima.Net.Extensions.Result;
using Optima.Net.Result;

namespace Optima.Net.Test
{
    public class ResultTWithTErrorTests
    {
        // --- Helper ---
        private record TestError(string Message);

        // --- Ok / Fail ---

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
            var result = Result<int, TestError>.Fail(5,error);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Error);
        }

        // --- Map ---

        [Fact]
        public void Map_ShouldTransformValue_WhenSuccess()
        {
            var result = Result<int, TestError>.Ok(2)
                .Map(x => x * 2);

            Assert.True(result.IsSuccess);
            Assert.Equal(4, result.Value);
        }

        [Fact]
        public void Map_ShouldNotExecute_WhenFailure()
        {
            var error = new TestError("Failed");
            var result = Result<int, TestError>.Fail(5, error)
                .Map(x => x * 2);

            Assert.True(result.IsFailure);
            Assert.Equal(error, result.Error);
        }

        // --- Bind ---

        [Fact]
        public void Bind_ShouldChain_WhenSuccess()
        {
            var result = Result<int, TestError>.Ok(10)
                .Bind(x => Result<string, TestError>.Ok($"Value:{x}"));

            Assert.True(result.IsSuccess);
            Assert.Equal("Value:10", result.Value);
        }

        [Fact]
        public void Bind_ShouldShortCircuit_WhenFailure()
        {
            var fail = Result<int, TestError>.Fail(5, new TestError("Oops"));
            var result = fail.Bind(x => Result<string, TestError>.Ok("ShouldNotRun"));

            Assert.True(result.IsFailure);
            Assert.Equal("Oops", result.Error.Message);
        }

        // --- Match ---

        [Fact]
        public void Match_ShouldReturnSuccessValue()
        {
            var result = Result<int, TestError>.Ok(5);

            var value = result.Match(
                x => x * 2,
                e => -1
            );

            Assert.Equal(10, value);
        }

        [Fact]
        public void Match_ShouldReturnFailureValue()
        {
            var error = new TestError("Fail");
            var result = Result<int, TestError>.Fail(5, error);

            var value = result.Match(
                x => x * 2,
                e => -1
            );

            Assert.Equal(-1, value);
        }

        // --- Ensure (sync) ---

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

        // --- EnsureAsync (Result<T> input) ---

        [Fact]
        public async Task EnsureAsync_ShouldPass_WhenPredicateTrue()
        {
            var result = await Result<int, TestError>.Ok(9)
                .EnsureAsync(async x => { await Task.Delay(5); return x == 9; },
                             () => new TestError("Not nine"));

            Assert.True(result.IsSuccess);
            Assert.Equal(9, result.Value);
        }

        [Fact]
        public async Task EnsureAsync_ShouldFail_WhenPredicateFalse()
        {
            var result = await Result<int, TestError>.Ok(3)
                .EnsureAsync(async x => { await Task.Delay(5); return x == 9; },
                             () => new TestError("Nope"));

            Assert.True(result.IsFailure);
            Assert.Equal("Nope", result.Error.Message);
        }

        // --- EnsureAsync (Task<Result<T>> pipeline) ---

        private async Task<Result<int, TestError>> GetNumberAsync(int value)
        {
            await Task.Delay(5);
            return Result<int, TestError>.Ok(value);
        }

        [Fact]
        public async Task EnsureAsync_TaskPipeline_ShouldPass()
        {
            var result = await GetNumberAsync(8)
                .EnsureAsync(x => Task.FromResult(x == 8),
                             () => new TestError("Wrong number"));

            Assert.True(result.IsSuccess);
            Assert.Equal(8, result.Value);
        }

        [Fact]
        public async Task EnsureAsync_TaskPipeline_ShouldFail()
        {
            var result = await GetNumberAsync(8)
                .EnsureAsync(x => Task.FromResult(x == 9),
                             () => new TestError("Wrong number"));

            Assert.True(result.IsFailure);
            Assert.Equal("Wrong number", result.Error.Message);
        }
    }
}
