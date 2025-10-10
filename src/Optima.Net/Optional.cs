using Optima.Net.Exceptions;

namespace Optima.Net
{
    public sealed class Optional<T> : IEquatable<Optional<T>>
    {
        private readonly T value;
        public bool HasValue { get; }

        // Singleton instance for None
        private static readonly Optional<T> none = new();

        private Optional() => HasValue = false;

        private Optional(T value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "Cannot wrap null in Some.");
            }

            this.value = value;
            HasValue = true;
        }

        // Factory methods
        public static Optional<T> None() => none;
        public static Optional<T> Some(T value) => new(value);

        /// <summary>
        /// Returns a NullValueException when trying to access a value that has not been set
        /// </summary>
        public T Value => HasValue ? value : throw NullValueException.ForType<T>();

        /// <summary>
        /// this is to allow for a default value to be returned if the value was not set 
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T ValueOrDefault(T defaultValue) => HasValue ? value : defaultValue;

        /// <summary>
        /// Returns a value of type T or a null if there is no value
        /// </summary>
        /// <returns>T?</returns>
        public T? ValueOrNull() => HasValue ? value : default;

        // Functional helpers
        public Optional<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            if (mapper is null) throw new ArgumentNullException(nameof(mapper));
            return HasValue ? Optional<TResult>.Some(mapper(value)) : Optional<TResult>.None();
        }

        public Optional<TResult> Bind<TResult>(Func<T, Optional<TResult>> binder)
        {
            if (binder is null) throw new ArgumentNullException(nameof(binder));
            return HasValue ? binder(value) : Optional<TResult>.None();
        }

        public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
        {
            if (onSome is null) throw new ArgumentNullException(nameof(onSome));
            if (onNone is null) throw new ArgumentNullException(nameof(onNone));
            return HasValue ? onSome(value) : onNone();
        }

        // Equality
        public bool Equals(Optional<T>? other)
        {
            if (other is null) return false;
            if (!HasValue && !other.HasValue) return true;
            if (HasValue != other.HasValue) return false;
            return EqualityComparer<T>.Default.Equals(value, other.value);
        }

        public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

        public override int GetHashCode() => HasValue ? value!.GetHashCode() : 0;

        public override string ToString() => HasValue ? $"Some({value})" : "None";
    }
}
