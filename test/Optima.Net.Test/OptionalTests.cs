using Optima.Net.Exceptions;

namespace Optima.Net.Tests
{
    public class OptionalTests
    {
        [Fact]
        public void None_ShouldReturnSingletonNone()
        {
            var none1 = Optional<int>.None();
            var none2 = Optional<int>.None();

            Assert.False(none1.HasValue);
            Assert.Same(none1, none2); // Singleton instance
        }

        [Fact]
        public void Some_ShouldWrapValue()
        {
            var some = Optional<int>.Some(42);

            Assert.True(some.HasValue);
            Assert.Equal(42, some.Value);
        }

        [Fact]
        public void Some_WithNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => Optional<string>.Some(null!));
        }

        [Fact]
        public void Value_WhenNone_ShouldThrowNullValueException()
        {
            var none = Optional<int>.None();

            Assert.Throws<NullValueException>(() => _ = none.Value);
        }

        [Fact]
        public void ValueOrDefault_ShouldReturnDefault_WhenNone()
        {
            var none = Optional<int>.None();

            int result = none.ValueOrDefault(99);

            Assert.Equal(99, result);
        }

        [Fact]
        public void ValueOrDefault_ShouldReturnValue_WhenSome()
        {
            var some = Optional<int>.Some(5);

            int result = some.ValueOrDefault(99);

            Assert.Equal(5, result);
        }

        [Fact]
        public void ValueOrNull_ShouldReturnNull_WhenNone()
        {
            var none = Optional<string>.None();

            string? result = none.ValueOrNull();

            Assert.Null(result);
        }

        [Fact]
        public void ValueOrNull_ShouldReturnValue_WhenSome()
        {
            var some = Optional<string>.Some("hello");

            string? result = some.ValueOrNull();

            Assert.Equal("hello", result);
        }

        [Fact]
        public void Map_ShouldApplyFunction_WhenSome()
        {
            var some = Optional<int>.Some(3);

            var result = some.Map(x => x * 2);

            Assert.True(result.HasValue);
            Assert.Equal(6, result.Value);
        }

        [Fact]
        public void Map_ShouldReturnNone_WhenNone()
        {
            var none = Optional<int>.None();

            var result = none.Map(x => x * 2);

            Assert.False(result.HasValue);
        }

        [Fact]
        public void Bind_ShouldApplyFunction_WhenSome()
        {
            var some = Optional<int>.Some(5);

            var result = some.Bind(x => Optional<string>.Some($"Value:{x}"));

            Assert.True(result.HasValue);
            Assert.Equal("Value:5", result.Value);
        }

        [Fact]
        public void Bind_ShouldReturnNone_WhenNone()
        {
            var none = Optional<int>.None();

            var result = none.Bind(x => Optional<string>.Some($"Value:{x}"));

            Assert.False(result.HasValue);
        }

        [Fact]
        public void Match_ShouldReturnOnSome_WhenSome()
        {
            var some = Optional<int>.Some(10);

            var result = some.Match(
                onSome: x => x * 2,
                onNone: () => 0
            );

            Assert.Equal(20, result);
        }

        [Fact]
        public void Match_ShouldReturnOnNone_WhenNone()
        {
            var none = Optional<int>.None();

            var result = none.Match(
                onSome: x => x * 2,
                onNone: () => 99
            );

            Assert.Equal(99, result);
        }

        [Fact]
        public void Equals_ShouldWorkCorrectly()
        {
            var a = Optional<int>.Some(5);
            var b = Optional<int>.Some(5);
            var c = Optional<int>.Some(10);
            var none1 = Optional<int>.None();
            var none2 = Optional<int>.None();

            Assert.True(a.Equals(b));
            Assert.False(a.Equals(c));
            Assert.True(none1.Equals(none2));
            Assert.False(a.Equals(none1));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void GetHashCode_ShouldMatchValueOrZero()
        {
            var some = Optional<int>.Some(42);
            var none = Optional<int>.None();

            Assert.Equal(42.GetHashCode(), some.GetHashCode());
            Assert.Equal(0, none.GetHashCode());
        }

        [Fact]
        public void ToString_ShouldReturnCorrectFormat()
        {
            var some = Optional<int>.Some(10);
            var none = Optional<int>.None();

            Assert.Equal("Some(10)", some.ToString());
            Assert.Equal("None", none.ToString());
        }
    }
}
