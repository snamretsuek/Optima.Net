using Optima.Net.Extensions.Result;
using Optima.Net.Result;

namespace Optima.Net.Test.Extensions.Result
{
    public class ResultCollectionExtensionTests
    {
        [Fact]
        public void Sequence_AllSuccess_ShouldReturnSuccessWithValues()
        {
            // Arrange
            var results = new[]
            {
                Result<int>.Ok(1),
                Result<int>.Ok(2),
                Result<int>.Ok(3)
            };

            // Act
            var aggregated = results.Sequence();

            // Assert
            Assert.True(aggregated.IsSuccess);
            Assert.Equal(new[] { 1, 2, 3 }, aggregated.Value.ToList());
            Assert.Equal(string.Empty, aggregated.Error);
        }

        [Fact]
        public void Sequence_WithFailures_ShouldReturnFailureWithAggregatedErrors()
        {
            // Arrange
            var results = new[]
            {
                Result<int>.Ok(10),
                Result<int>.Fail("Bad A"),
                Result<int>.Fail("Bad B")
            };

            // Act
            var aggregated = results.Sequence();

            // Assert
            Assert.True(aggregated.IsFailure);
            Assert.Contains("Bad A", aggregated.Error);
            Assert.Contains("Bad B", aggregated.Error);
        }

        [Fact]
        public async Task SequenceAsync_AllSuccess_ShouldReturnSuccess()
        {
            // Arrange
            var tasks = new[]
            {
                Task.FromResult(Result<string>.Ok("A")),
                Task.FromResult(Result<string>.Ok("B"))
            };

            // Act
            var result = await tasks.SequenceAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("A", result.Value);
            Assert.Contains("B", result.Value);
        }

        [Fact]
        public async Task SequenceAsync_WithFailures_ShouldAggregateErrors()
        {
            // Arrange
            var tasks = new[]
            {
                Task.FromResult(Result<int>.Fail("First")),
                Task.FromResult(Result<int>.Fail("Second"))
            };

            // Act
            var result = await tasks.SequenceAsync();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("First", result.Error);
            Assert.Contains("Second", result.Error);
        }

        [Fact]
        public async Task SequenceAsync_ShouldRespectCancellation()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var tasks = new[]
            {
                Task.FromResult(Result<int>.Ok(1))
            };

            // Act + Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => tasks.SequenceAsync(cts.Token));
        }

        [Fact]
        public void Where_ShouldFilterResultsByPredicate()
        {
            // Arrange
            var results = new[]
            {
                Result<int>.Ok(1),
                Result<int>.Ok(2),
                Result<int>.Ok(3)
            };

            // Act
            var filtered = results.Where(x => x > 1).ToList();

            // Assert
            Assert.Equal(3, filtered.Count);
            Assert.True(filtered[0].IsFailure); // 1 filtered out
            Assert.True(filtered[1].IsSuccess);
            Assert.True(filtered[2].IsSuccess);
        }

        [Fact]
        public async Task WhereAsync_ShouldFilterResultsByAsyncPredicate()
        {
            // Arrange
            var results = new[]
            {
                Result<int>.Ok(1),
                Result<int>.Ok(2),
                Result<int>.Fail("bad")
            };

            async Task<bool> Predicate(int x, CancellationToken _) => await Task.FromResult(x % 2 == 0);

            // Act
            var filtered = await results.WhereAsync(Predicate);

            // Assert
            var list = filtered.ToList();
            Assert.Equal(3, list.Count);
            Assert.True(list[0].IsFailure); // filtered out
            Assert.True(list[1].IsSuccess);
            Assert.True(list[2].IsFailure); // original failure preserved
        }

        [Fact]
        public void Flatten_ShouldUnwrapInnerResult()
        {
            // Arrange
            var inner = Result<string>.Ok("inner");
            var outer = Result<Result<string>>.Ok(inner);

            // Act
            var flat = outer.Flatten();

            // Assert
            Assert.True(flat.IsSuccess);
            Assert.Equal("inner", flat.Value);
        }

        [Fact]
        public void Flatten_ShouldPropagateOuterFailure()
        {
            // Arrange
            var outer = Result<Result<int>>.Fail("outer fail");

            // Act
            var flat = outer.Flatten();

            // Assert
            Assert.True(flat.IsFailure);
            Assert.Equal("outer fail", flat.Error);
        }
    }
}
