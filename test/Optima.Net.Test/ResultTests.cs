using Optima.Net.Exceptions;
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
            Assert.Null(result.Error);
        }

        [Fact]
        public void Fail_ShouldCreateFailureResult_WithGivenError()
        {
            // Arrange
            var error = "Something went wrong.";

            // Act
            var result = Result<string>.Fail(error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(default(string), result.Value);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Ok_ShouldThrowNullValueException_WhenValueIsNull()
        {
            // Act & Assert
            var ex = Assert.Throws<NullValueException>(() => Result<string>.Ok(null!));
            Assert.Equal("Result<String> has no value.", ex.Message);
        }

        [Fact]
        public void Fail_ShouldDefaultValueType_WhenFailed()
        {
            // Act
            var result = Result<int>.Fail("bad things happened");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(default(int), result.Value);
            Assert.Equal("bad things happened", result.Error);
        }

        [Fact]
        public void IsFailure_ShouldBeInverseOfIsSuccess()
        {
            var okResult = Result<bool>.Ok(true);
            var failResult = Result<bool>.Fail("error");

            Assert.False(okResult.IsFailure);
            Assert.True(failResult.IsFailure);
        }
    }
}
