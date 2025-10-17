using Optima.Net.Exceptions;
using Optima.Net.Extensions;

namespace Optima.Net.Test.Extensions
{
    public class OptionalExtensionsTests
    {
        [Fact]
        public void ValueOrThrow_ShouldReturnValue_WhenSome()
        {
            var some = Optional<int>.Some(42);
            var result = some.ValueOrThrow();
            Assert.Equal(42, result);
        }

        [Fact]
        public void ValueOrThrow_ShouldThrowCustomMessage_WhenNone()
        {
            var none = Optional<int>.None();
            var ex = Assert.Throws<NullValueException>(() => none.ValueOrThrow("Custom message"));
            Assert.Equal("Custom message", ex.Message);
        }

        [Fact]
        public void Tap_ShouldExecuteAction_WhenSome()
        {
            var some = Optional<int>.Some(5);
            var called = false;

            var result = some.Tap(x => { called = true; Assert.Equal(5, x); });

            Assert.True(called);
            Assert.Equal(some, result);
        }

        [Fact]
        public async Task TapAsync_ShouldExecuteAsyncAction_WhenSome()
        {
            var some = Optional<int>.Some(10);
            var called = false;

            var result = await some.TapAsync(async x =>
            {
                await Task.Delay(1);
                called = x == 10;
            });

            Assert.True(called);
            Assert.Equal(some, result);
        }

        [Fact]
        public void Where_ShouldReturnOptional_WhenPredicateTrue()
        {
            var some = Optional<int>.Some(5);
            var result = some.Where(x => x > 0);
            Assert.True(result.HasValue);
        }

        [Fact]
        public void Where_ShouldReturnNone_WhenPredicateFalse()
        {
            var some = Optional<int>.Some(5);
            var result = some.Where(x => x < 0);
            Assert.False(result.HasValue);
        }

        [Fact]
        public async Task WhereAsync_ShouldReturnOptional_WhenPredicateTrue()
        {
            var some = Optional<int>.Some(5);
            var result = await some.WhereAsync(async x =>
            {
                await Task.Delay(1);
                return x > 0;
            });

            Assert.True(result.HasValue);
        }

        [Fact]
        public async Task WhereAsync_ShouldReturnNone_WhenPredicateFalse()
        {
            var some = Optional<int>.Some(5);
            var result = await some.WhereAsync(async x =>
            {
                await Task.Delay(1);
                return x < 0;
            });

            Assert.False(result.HasValue);
        }

        [Fact]
        public void Or_ShouldReturnOriginal_WhenHasValue()
        {
            var some = Optional<int>.Some(1);
            var fallback = Optional<int>.Some(2);
            var result = some.Or(fallback);
            Assert.Equal(some, result);
        }

        [Fact]
        public void Or_ShouldReturnFallback_WhenNone()
        {
            var none = Optional<int>.None();
            var fallback = Optional<int>.Some(2);
            var result = none.Or(fallback);
            Assert.Equal(fallback, result);
        }

        [Fact]
        public async Task OrAsync_ShouldReturnFallback_WhenNone()
        {
            var none = Optional<int>.None();
            var result = await none.OrAsync(async () =>
            {
                await Task.Delay(1);
                return Optional<int>.Some(3);
            });

            Assert.Equal(3, result.Value);
        }

        [Fact]
        public async Task MatchAsync_ShouldCallOnSome_WhenSome()
        {
            var some = Optional<int>.Some(5);
            var called = false;
            await some.MatchAsync(async x =>
            {
                await Task.Delay(1);
                called = x == 5;
            }, async () => { Assert.Fail("Should not call onNone"); });

            Assert.True(called);
        }

        [Fact]
        public async Task MatchAsync_ShouldCallOnNone_WhenNone()
        {
            var none = Optional<int>.None();
            var called = false;
            await none.MatchAsync(async x => { Assert.Fail("Should not call onSome"); }, async () =>
            {
                await Task.Delay(1);
                called = true;
            });

            Assert.True(called);
        }

        [Fact]
        public async Task MapAsync_ShouldReturnMappedOptional_WhenSome()
        {
            var some = Optional<int>.Some(3);
            var result = await some.MapAsync(async x =>
            {
                await Task.Delay(1);
                return x * 2;
            });

            Assert.True(result.HasValue);
            Assert.Equal(6, result.Value);
        }

        [Fact]
        public async Task BindAsync_ShouldReturnBoundOptional_WhenSome()
        {
            var some = Optional<int>.Some(5);
            var result = await some.BindAsync(async x =>
            {
                await Task.Delay(1);
                return Optional<int>.Some(x + 1);
            });

            Assert.True(result.HasValue);
            Assert.Equal(6, result.Value);
        }

        [Fact]
        public void TryGetValue_ShouldReturnTrue_WhenSome()
        {
            var some = Optional<int>.Some(7);
            var success = some.TryGetValue(out var value);
            Assert.True(success);
            Assert.Equal(7, value);
        }

        [Fact]
        public void TryGetValue_ShouldReturnFalse_WhenNone()
        {
            var none = Optional<int>.None();
            var success = none.TryGetValue(out var value);
            Assert.False(success);
            Assert.Equal(default, value);
        }

        [Fact]
        public void Zip_ShouldReturnCombinedOptional_WhenBothSome()
        {
            var a = Optional<int>.Some(2);
            var b = Optional<int>.Some(3);
            var result = a.Zip(b, (x, y) => x + y);
            Assert.True(result.HasValue);
            Assert.Equal(5, result.Value);
        }

        [Fact]
        public void Zip_ShouldReturnNone_WhenEitherNone()
        {
            var a = Optional<int>.Some(2);
            var b = Optional<int>.None();
            var result = a.Zip(b, (x, y) => x + y);
            Assert.False(result.HasValue);
        }

        [Fact]
        public void ToEnumerable_ShouldReturnSingleValue_WhenSome()
        {
            var some = Optional<string>.Some("hello");
            var result = some.ToEnumerable().ToList();
            Assert.Single(result);
            Assert.Equal("hello", result[0]);
        }

        [Fact]
        public void ToEnumerable_ShouldReturnEmpty_WhenNone()
        {
            var none = Optional<string>.None();
            var result = none.ToEnumerable().ToList();
            Assert.Empty(result);
        }
    }
}
