using Optima.Net.Extensions.Result;
using Optima.Net.Result;

namespace Optima.Net.Test.Extensions.Result
{
    public class ResultExtensionsTests
    {

        [Fact]
        public void Bind_ShouldReturnFailure_IfSourceIsFailure()
        {
            var result = Result<int>.Fail("fail");
            var bound = result.Bind(x => Result<string>.Ok(x.ToString()));

            Assert.True(bound.IsFailure);
            Assert.Equal("fail", bound.Error);
        }

        [Fact]
        public void Bind_ShouldInvokeFunc_WhenSuccess()
        {
            var result = Result<int>.Ok(42);
            var bound = result.Bind(x => Result<string>.Ok($"Value: {x}"));

            Assert.True(bound.IsSuccess);
            Assert.Equal("Value: 42", bound.Value);
        }

        [Fact]
        public async Task BindAsync_ShouldReturnFailure_IfSourceFails()
        {
            var result = Result<int>.Fail("boom");
            var bound = await result.BindAsync(x => Task.FromResult(Result<string>.Ok($"ok {x}")));

            Assert.True(bound.IsFailure);
            Assert.Equal("boom", bound.Error);
        }

        [Fact]
        public async Task BindAsync_ShouldReturnSuccess_WhenFuncSucceeds()
        {
            var result = Result<int>.Ok(5);
            var bound = await result.BindAsync(x => Task.FromResult(Result<string>.Ok($"#{x}")));

            Assert.True(bound.IsSuccess);
            Assert.Equal("#5", bound.Value);
        }

        [Fact]
        public async Task BindAsync_WithCancellationToken_ShouldPassThrough()
        {
            var result = Result<int>.Ok(99);
            var bound = await result.BindAsync((x, ct) => Task.FromResult(Result<string>.Ok($"Val: {x}")));

            Assert.True(bound.IsSuccess);
            Assert.Equal("Val: 99", bound.Value);
        }

        [Fact]
        public void Map_ShouldReturnFailure_WhenSourceFails()
        {
            var result = Result<int>.Fail("error");
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
            var result = Result<int>.Fail("err");
            var mapped = await result.MapAsync(x => Task.FromResult(x + 1));

            Assert.True(mapped.IsFailure);
            Assert.Equal("err", mapped.Error);
        }

        [Fact]
        public async Task MapAsync_ShouldReturnSuccess_WhenSourceSucceeds()
        {
            var result = Result<int>.Ok(5);
            var mapped = await result.MapAsync(x => Task.FromResult(x + 10));

            Assert.True(mapped.IsSuccess);
            Assert.Equal(15, mapped.Value);
        }

        [Fact]
        public async Task MapAsync_WithCancellationToken_ShouldReturnSuccess()
        {
            var result = Result<int>.Ok(7);
            var mapped = await result.MapAsync((x, ct) => Task.FromResult(x * 3));

            Assert.True(mapped.IsSuccess);
            Assert.Equal(21, mapped.Value);
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
            var result = Result<int>.Fail("bad");
            bool executed = false;

            result.Tap(_ => executed = true);

            Assert.False(executed);
        }

        [Fact]
        public async Task TapAsync_ShouldExecute_OnSuccess()
        {
            var result = Result<string>.Ok("yo");
            bool executed = false;

            var returned = await result.TapAsync(async v =>
            {
                await Task.Delay(10);
                executed = true;
            });

            Assert.True(executed);
            Assert.Same(result, returned);
        }

        [Fact]
        public async Task TapAsync_ShouldNotExecute_OnFailure()
        {
            var result = Result<string>.Fail("nope");
            bool executed = false;

            await result.TapAsync(async v =>
            {
                executed = true;
                await Task.CompletedTask;
            });

            Assert.False(executed);
        }

        [Fact]
        public async Task TapAsync_WithCancellationToken_ShouldExecute()
        {
            var result = Result<int>.Ok(3);
            bool executed = false;

            await result.TapAsync(async (v, ct) =>
            {
                executed = true;
                await Task.CompletedTask;
            });

            Assert.True(executed);
        }

        [Fact]
        public void OnFailure_ShouldExecuteAction_WhenFailure()
        {
            var result = Result<int>.Fail("oops");
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
            var output = result.Match(v => v * 2, e => -1);

            Assert.Equal(10, output);
        }

        [Fact]
        public void Match_ShouldInvokeFailureBranch()
        {
            var result = Result<int>.Fail("fail");
            var output = result.Match(v => v * 2, e => -1);

            Assert.Equal(-1, output);
        }

        [Fact]
        public async Task MatchAsync_ShouldInvokeSuccessBranch()
        {
            var result = Result<int>.Ok(10);
            var output = await result.MatchAsync(
                v => Task.FromResult(v * 3),
                e => Task.FromResult(-1)
            );

            Assert.Equal(30, output);
        }

        [Fact]
        public async Task MatchAsync_ShouldInvokeFailureBranch()
        {
            var result = Result<int>.Fail("bad");
            var output = await result.MatchAsync(
                v => Task.FromResult(v * 3),
                e => Task.FromResult(-1)
            );

            Assert.Equal(-1, output);
        }

        [Fact]
        public async Task MatchAsync_WithCancellationToken_ShouldWork()
        {
            var result = Result<int>.Ok(7);
            var output = await result.MatchAsync(
                (v, ct) => Task.FromResult(v * 2),
                (e, ct) => Task.FromResult(-1)
            );

            Assert.Equal(14, output);
        }
    }
}
