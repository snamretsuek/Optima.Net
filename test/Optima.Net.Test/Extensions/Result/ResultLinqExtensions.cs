using Optima.Net.Extensions.Result.LINQ;
using Optima.Net.Result;


namespace Optima.Net.Test.Extensions.Result
{
    public class ResultLinqExtensionsTests
    {
        [Fact]
        public void Select_Should_Map_Success_Value()
        {
            // Arrange
            var result = Result<int>.Ok(10);

            // Act
            var mapped = result.Select(x => x * 2);

            // Assert
            Assert.True(mapped.IsSuccess);
            Assert.Equal(20, mapped.Value);
        }

        [Fact]
        public void Select_Should_Propagate_Failure()
        {
            // Arrange
            var result = Result<int>.Fail(5, "Bad math");

            // Act
            var mapped = result.Select(x => x * 2);

            // Assert
            Assert.True(mapped.IsFailure);
            Assert.Equal("Bad math", mapped.Error);
        }

        [Fact]
        public void SelectMany_Should_Compose_Successful_Results()
        {
            // Arrange
            var r1 = Result<int>.Ok(3);

            // Act
            var composed = r1.SelectMany(
                x => Result<int>.Ok(x + 2),
                (x, y) => x * y);

            // Assert
            Assert.True(composed.IsSuccess);
            Assert.Equal(15, composed.Value);
        }

        [Fact]
        public void SelectMany_Should_Return_Failure_If_Any_Fails()
        {
            // Arrange
            var r1 = Result<int>.Ok(5);
            var r2 = Result<int>.Fail(5, "boom");

            // Act
            var composed = r1.SelectMany(
                x => r2,
                (x, y) => x * y);

            // Assert
            Assert.True(composed.IsFailure);
            Assert.Equal("boom", composed.Error);
        }

        [Fact]
        public void Where_Should_Keep_Successes_Matching_Predicate()
        {
            // Arrange
            var results = new[]
            {
                Result<int>.Ok(10),
                Result<int>.Ok(3),
                Result<int>.Ok(7)
            };

            // Act
            var filtered = results.Where(x => x > 5).ToList();

            // Assert
            Assert.Equal(3, filtered.Count);
            Assert.True(filtered[0].IsSuccess);
            Assert.Equal(10, filtered[0].Value);
            Assert.True(filtered[1].IsFailure); // 3 filtered out
            Assert.True(filtered[2].IsSuccess);
            Assert.Equal(7, filtered[2].Value);
        }

        [Fact]
        public void Where_Should_Preserve_Failures()
        {
            // Arrange
            var results = new[]
            {
                Result<int>.Fail(5, "bad"),
                Result<int>.Ok(4)
            };

            // Act
            var filtered = results.Where(x => x > 10).ToList();

            // Assert
            Assert.True(filtered[0].IsFailure); // same failure
            Assert.Equal("bad", filtered[0].Error);
            Assert.True(filtered[1].IsFailure); // filtered out
            Assert.Equal("Filtered out", filtered[1].Error);
        }

        [Fact]
        public void Linq_Query_Syntax_Should_Work()
        {
            // Arrange
            var r1 = Result<int>.Ok(2);
            var r2 = Result<int>.Ok(3);

            // Act (this uses LINQ query comprehension)
            var query =
                from a in r1
                from b in r2
                select a + b;

            // Assert
            Assert.True(query.IsSuccess);
            Assert.Equal(5, query.Value);
        }

        [Fact]
        public void Linq_Query_Syntax_Should_ShortCircuit_On_Failure()
        {
            // Arrange
            var r1 = Result<int>.Ok(2);
            var r2 = Result<int>.Fail(5, "nope");

            // Act
            var query =
                from a in r1
                from b in r2
                select a + b;

            // Assert
            Assert.True(query.IsFailure);
            Assert.Equal("nope", query.Error);
        }
    }
}
