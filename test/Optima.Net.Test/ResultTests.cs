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

            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = result.Error;
            });
        }

        [Fact]
        public void Fail_ShouldCreateFailureResult_WithGivenError()
        {
            // Arrange
            var error = "Something went wrong.";

            // Act
            var result = Result<string>.Fail("Bad",error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal("Bad", result.Value);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Ok_ShouldThrowNullValueException_WhenValueIsNull()
        {
            // Act
            var ex = Assert.Throws<NullValueException>(() => Result<string>.Ok(null!));

            // Assert
            Assert.Equal("Result<String> has no value.", ex.Message);
            //Assert.Contains("Result", ex.Message);
            //Assert.Contains("value", ex.Message);
        }

        [Fact]
        public void Fail_ShouldDefaultValueType_WhenFailed()
        {
            // Act
            var result = Result<int>.Fail(5, "bad things happened");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(5, result.Value);
            Assert.Equal("bad things happened", result.Error);
        }

        [Fact]
        public void IsFailure_ShouldBeInverseOfIsSuccess()
        {
            var okResult = Result<bool>.Ok(true);
            var failResult = Result<bool>.Fail(false,"error");

            Assert.False(okResult.IsFailure);
            Assert.True(failResult.IsFailure);
        }
    }
}
