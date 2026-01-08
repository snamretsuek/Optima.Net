using Optima.Net.Extensions.Result;
using Optima.Net.Result;
using Xunit;
using System.Threading.Tasks;

namespace Optima.Net.Test.Extensions.Result
{
    public class ResultExtensionsTests
    {
        [Fact]
        public void Bind_ShouldReturnFailure_IfSourceIsFailure()
        {
            var result = Result<int>.Fail(5, "fail");

            var bound = result.Bind(x =>
                Result<string>.Ok(x.ToString()));

            Assert.True(bound.IsFailure);
            Assert.Equal("fail", bound.Error);
        }

        [Fact]
        public void Bind_ShouldInvokeFunc_WhenSuccess()
        {
            var result = Result<int>.Ok(42);

            var bound = result.Bind(x =>
                Result<string>.Ok($"Value: {x}"));

            Assert.True(bound.IsSuccess);
            Assert.Equal("Value: 42", bound.Value);
        }

        [Fact]
        public async Task BindAsync_ShouldReturnFailure_IfSourceFails()
        {
            var result = Result<int>.Fail(5, "boom");

            var bound = await result.BindAsync(x =>
                Task.FromResult(Result<string>.Ok($"ok {x}")));

            Assert.True(bound.IsFailure);
            Assert.Equal("boom", bound.Error);
        }

        [Fact]
        public async Task BindAsync_ShouldReturnSuccess_WhenFuncSucceeds()
        {
            var result = Result<int>.Ok(5);

            var bound = await result.BindAsync(x =>
                Task.FromResult(Result<string>.Ok($"#{x}")));

            Assert.True(bound.IsSuccess);
            Assert.Equal("#5", bound.Value);
        }

        [Fact]
        public void Map_ShouldReturnFailure_WhenSourceFails()
        {
            var result = Result<int>.Fail(5, "error");

            var mapped = result.Map(x => x * 2);

            Assert.True(mapped.IsFailure);
            Assert.Equal("error", mapped.Error);
        }

        [Fact]
        public void Map_ShouldApplyFunction_WhenSuccess()
        {
            var result = Result<int>.Ok(10);

            var mapped = result.Map(x => x * 2);

            Assert.True(mapped.IsSuccess);
            Assert.Equal(20, mapped.Value);
        }

        [Fact]
        public async Task MapAsync_ShouldReturnFailure_WhenSourceFails()
        {
            var result = Result<int>.Fail(5, "err");

            var mapped = await result.MapAsync(x =>
                Task.FromResult(x + 1));

            Assert.True(mapped.IsFailure);
            Assert.Equal("err", mapped.Error);
        }

        [Fact]
        public async Task MapAsync_ShouldReturnSuccess_WhenSourceSucceeds()
        {
            var result = Result<int>.Ok(5);

            var mapped = await result.MapAsync(x =>
                Task.FromResult(x + 10));

            Assert.True(mapped.IsSuccess);
            Assert.Equal(15, mapped.Value);
        }

        [Fact]
        public void Tap_ShouldExecuteAction_OnSuccess()
        {
            var result = Result<int>.Ok(42);
            int sideEffect = 0;

            var returned = result.Tap(v => sideEffect = v + 1);

            Assert.Equal(43, sideEffect);
            Assert.Same(result, returned);
        }

        [Fact]
        public void Tap_ShouldNotExecute_OnFailure()
        {
            var result = Result<int>.Fail(5, "bad");
            bool executed = false;

            result.Tap(_ => executed = true);

            Assert.False(executed);
        }

        [Fact]
        public void OnFailure_ShouldExecuteAction_WhenFailure()
        {
            var result = Result<int>.Fail(5, "oops");
            string message = "";

            result.OnFailure(e => message = e);

            Assert.Equal("oops", message);
        }

        [Fact]
        public void OnFailure_ShouldNotExecute_WhenSuccess()
        {
            var result = Result<int>.Ok(10);
            bool called = false;

            result.OnFailure(_ => called = true);

            Assert.False(called);
        }

        [Fact]
        public void Match_ShouldInvokeSuccessBranch()
        {
            var result = Result<int>.Ok(5);

            var output = result.Match(
                onSuccess: v => v * 2,
                onFailure: _ => -1);

            Assert.Equal(10, output);
        }

        [Fact]
        public void Match_ShouldInvokeFailureBranch()
        {
            var result = Result<int>.Fail(5, "fail");

            var output = result.Match(
                onSuccess: v => v * 2,
                onFailure: _ => -1);

            Assert.Equal(-1, output);
        }

        [Fact]
        public async Task MatchAsync_ShouldInvokeSuccessBranch()
        {
            var result = Result<int>.Ok(10);

            var output = await result.MatchAsync(
                onSuccess: v => Task.FromResult(v * 3),
                onFailure: _ => Task.FromResult(-1));

            Assert.Equal(30, output);
        }

        [Fact]
        public async Task MatchAsync_ShouldInvokeFailureBranch()
        {
            var result = Result<int>.Fail(5, "bad");

            var output = await result.MatchAsync(
                onSuccess: v => Task.FromResult(v * 3),
                onFailure: _ => Task.FromResult(-1));

            Assert.Equal(-1, output);
        }
    }
}
