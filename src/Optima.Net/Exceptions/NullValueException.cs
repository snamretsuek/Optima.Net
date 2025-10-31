namespace Optima.Net.Exceptions
{
    public sealed class NullValueException:Exception
    {
        /// <summary>
        /// I don't do these very often but I need to explain why I created this exception.
        /// This exception has been created in case we want to access the value of Optional<T>
        /// by calling Optional<T>.Value, when it wasn't set.
        /// It is just more descriptive in  y opinion
        /// </summary>
        public NullValueException() : base("A required value is null or missing.") { }
        public NullValueException(string? message) : base(message) { }
        public NullValueException(string? message, Exception? innerException) : base(message, innerException) { }
        public NullValueException(string?paramName, string? message)
            : base(message is null ? $"The parameter '{paramName}' is null or missing." : message){}

        /// <summary>
        /// The above is so that you can use the exception every where if you wanted to
        /// The Below is specific to the Optional<T> or Result<T>use case.
        /// </summary>
        /// 
        [Obsolete("Use ForOptionalType<T>() or ForResultType<T>() instead.")]
        public static NullValueException ForType<T>() => new($"<{typeof(T).Name}> has no value.");// for backward copatibility

        public static NullValueException ForOptionalType<T>() => new($"Optional<{typeof(T).Name}> has no value.");

        public static NullValueException ForResultType<T>() => new($"Result<{typeof(T).Name}> has no value.");
    }
}
