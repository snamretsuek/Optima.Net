using Optima.Net.Extensions.Collections;

namespace Optima.Net.Test.Extensions
{
    public class OptionalCollectionExtensionsTests
    {
        [Fact]
        public void Where_ShouldFilterOptionalsBasedOnPredicate()
        {
            var optionals = new[]
            {
                Optional<int>.Some(1),
                Optional<int>.Some(2),
                Optional<int>.None(),
                Optional<int>.Some(3)
            };

            var result = optionals.Where(x => x % 2 == 1).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, o => Assert.True(o.HasValue && o.Value % 2 == 1));
        }

        [Fact]
        public async Task WhereAsync_ShouldFilterOptionalsBasedOnAsyncPredicate()
        {
            var optionals = new[]
            {
                Optional<int>.Some(1),
                Optional<int>.Some(2),
                Optional<int>.None(),
                Optional<int>.Some(3)
            };

            var result = (await optionals.WhereAsync(async x =>
            {
                await Task.Delay(10); // simulate async
                return x % 2 == 0;
            })).ToList();

            Assert.Single(result);
            Assert.True(result[0].HasValue && result[0].Value == 2);
        }

        [Fact]
        public async Task WhereAsync_WithCancellation_ShouldFilterOptionals()
        {
            var optionals = new[]
            {
                Optional<int>.Some(5),
                Optional<int>.Some(6),
                Optional<int>.None(),
                Optional<int>.Some(7)
            };

            var cancellationToken = new CancellationTokenSource().Token;

            var result = (await optionals.WhereAsync(async (x, ct) =>
            {
                await Task.Delay(10, ct);
                return x > 5;
            }, cancellationToken)).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, o => Assert.True(o.HasValue && o.Value > 5));
        }

        [Fact]
        public void Flatten_ShouldReturnOnlyValuesFromOptionals()
        {
            var optionals = new[]
            {
                Optional<string>.Some("a"),
                Optional<string>.None(),
                Optional<string>.Some("b")
            };

            var result = optionals.Flatten().ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains("a", result);
            Assert.Contains("b", result);
        }

        [Fact]
        public void Where_ShouldReturnEmpty_WhenNoOptionalsMatch()
        {
            var optionals = new[]
            {
                Optional<int>.Some(1),
                Optional<int>.Some(2),
                Optional<int>.None()
            };

            var result = optionals.Where(x => x > 100).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task WhereAsync_ShouldReturnEmpty_WhenNoOptionalsMatch()
        {
            var optionals = new[]
            {
                Optional<int>.Some(1),
                Optional<int>.Some(2),
                Optional<int>.None()
            };

            var result = (await optionals.WhereAsync(async x =>
            {
                await Task.Delay(1);
                return x > 100;
            })).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Flatten_ShouldReturnEmpty_WhenAllOptionalsAreNone()
        {
            var optionals = new[]
            {
                Optional<int>.None(),
                Optional<int>.None()
            };

            var result = optionals.Flatten().ToList();

            Assert.Empty(result);
        }
    }
}
