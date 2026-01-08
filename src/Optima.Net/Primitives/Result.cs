using Optima.Net.Primitives;

namespace Optima.Net.Result
{
    /// <summary>
    /// Convenience helpers for Result&lt;Unit&gt;
    /// </summary>
    public static class Result
    {
        /// <summary>
        /// Successful Result with no meaningful value.
        /// </summary>
        public static Result<Unit> Ok() =>
            Result<Unit>.Ok(Unit.Value);

        /// <summary>
        /// Failed Result with no meaningful value.
        /// Backward-compatible legacy overload.
        /// </summary>
        public static Result<Unit> Fail(string error) =>
            Result<Unit>.Fail(Unit.Value, error);
    }
}
