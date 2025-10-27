using Optima.Net.Extensions.LINQ;

namespace Optima.Net.Test.Extensions
{
    public class OptionalLinqExtensionsTests
    {
        [Fact]
        public void Select_ShouldMapValue_WhenSome()
        {
            var some = Optional<int>.Some(5);
            var result = some.Select(x => x * 2);

            Assert.True(result.HasValue);
            Assert.Equal(10, result.Value);
        }

        [Fact]
        public void Select_ShouldReturnNone_WhenNone()
        {
            var none = Optional<int>.None();
            var result = none.Select(x => x * 2);

            Assert.False(result.HasValue);
        }

        [Fact]
        public void SelectMany_ShouldProjectValue_WhenBothSome()
        {
            var some = Optional<int>.Some(3);

            var result = some.SelectMany(
                x => Optional<int>.Some(x + 2),
                (x, y) => x * y
            );

            Assert.True(result.HasValue);
            Assert.Equal(15, result.Value); // 3 * (3+2)
        }

        [Fact]
        public void SelectMany_ShouldReturnNone_WhenOuterNone()
        {
            var none = Optional<int>.None();

            var result = none.SelectMany(
                x => Optional<int>.Some(x + 2),
                (x, y) => x * y
            );

            Assert.False(result.HasValue);
        }

        [Fact]
        public void SelectMany_ShouldReturnNone_WhenInnerNone()
        {
            var some = Optional<int>.Some(3);

            var result = some.SelectMany(
                x => Optional<int>.None(),
                (x, y) => x * y
            );

            Assert.False(result.HasValue);
        }

        [Fact]
        public void Where_ShouldReturnOptional_WhenPredicateTrue()
        {
            var some = Optional<int>.Some(5);
            var result = some.Where(x => x > 0);

            Assert.True(result.HasValue);
            Assert.Equal(5, result.Value);
        }

        [Fact]
        public void Where_ShouldReturnNone_WhenPredicateFalse()
        {
            var some = Optional<int>.Some(5);
            var result = some.Where(x => x < 0);

            Assert.False(result.HasValue);
        }

        [Fact]
        public void Where_ShouldReturnNone_WhenOptionalIsNone()
        {
            var none = Optional<int>.None();
            var result = none.Where(x => x > 0);

            Assert.False(result.HasValue);
        }
    }
}
