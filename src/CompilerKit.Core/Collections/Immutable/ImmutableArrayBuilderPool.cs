using CompilerKit.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace CompilerKit.Collections.Immutable
{
    /// <summary>
    /// Represents a factory for <see cref="ImmutableArray{T}.Builder"/> instances.
    /// </summary>
    public static class ImmutableArrayBuilderPool
    {
        private static class PoolContainer<T>
        {
            public static readonly ObjectPool<ImmutableArray<T>.Builder> Pool =
                new ObjectPool<ImmutableArray<T>.Builder>(ImmutableArray.CreateBuilder<T>);
        }

        /// <summary>
        /// Allocates a builder.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <returns>The builder.</returns>
        public static ImmutableArray<T>.Builder Allocate<T>() => PoolContainer<T>.Pool.Allocate();

        /// <summary>
        /// Allocates a builder.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <returns>The builder.</returns>
        public static ImmutableArray<T>.Builder Allocate<T>(int size)
        {
            var result = Allocate<T>();
            result.Capacity = size;
            return result;
        }

        /// <summary>
        /// Frees the specified builder.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="builder">The builder to free.</param>
        public static void Free<T>(this ImmutableArray<T>.Builder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (builder.Count != 0) builder.Clear();
            PoolContainer<T>.Pool.Free(builder);
        }

        /// <summary>
        /// Converts the specified <see cref="ImmutableArray{T}.Builder"/> to a <see cref="ImmutableArray{T}"/>
        /// and frees it.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="builder">The builder to free.</param>
        /// <returns>The <see cref="ImmutableArray{T}"/>.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static ImmutableArray<T> ToImmutableAndFree<T>(this ImmutableArray<T>.Builder builder)
        {
            if (builder == null) return default(ImmutableArray<T>);
            var result = builder.ToImmutable();
            Free(builder);
            return result;
        }
    }
}
