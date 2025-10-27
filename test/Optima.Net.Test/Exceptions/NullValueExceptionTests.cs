using Optima.Net.Exceptions;

namespace Optima.Net.Test.Exceptions
{
    public class NullValueExceptionTests
    {
        [Fact]
        public void DefaultConstructor_ShouldHaveDefaultMessage()
        {
            var ex = new NullValueException();
            Assert.Equal("A required value is null or missing.", ex.Message);
        }

        [Fact]
        public void Constructor_WithMessage_ShouldSetMessage()
        {
            var ex = new NullValueException("Custom message");
            Assert.Equal("Custom message", ex.Message);
        }

        [Fact]
        public void Constructor_WithMessageAndInnerException_ShouldSetProperties()
        {
            var inner = new InvalidOperationException();
            var ex = new NullValueException("Error occurred", inner);

            Assert.Equal("Error occurred", ex.Message);
            Assert.Equal(inner, ex.InnerException);
        }

        [Fact]
        public void ForType_ShouldReturnCorrectMessage()
        {
            var ex = NullValueException.ForOptionalType<int>();
            Assert.IsType<NullValueException>(ex);
            Assert.Equal("Optional<Int32> has no value.", ex.Message);

            var ex2 = NullValueException.ForOptionalType<string>();
            Assert.Equal("Optional<String> has no value.", ex2.Message);
        }
    }
}
