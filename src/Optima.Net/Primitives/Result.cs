using Optima.Net.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optima.Net.Primitives
{
    public static class Result
    {
        public static Result<Unit> Ok()
            => Result<Unit>.Ok(Unit.Value);

        public static Result<Unit> Fail(string error)
            => Result<Unit>.Fail(error);
    }
}
