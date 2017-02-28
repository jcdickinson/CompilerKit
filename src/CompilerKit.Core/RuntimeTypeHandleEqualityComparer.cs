using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompilerKit
{
    /// <summary>
    /// Represents a <see cref="IEqualityComparer{RuntimeTypeHandle}" /> that provides functions suitable
    /// for using <see cref="RuntimeTypeHandle" /> in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEqualityComparer{System.RuntimeTypeHandle}" />
    public sealed class RuntimeTypeHandleEqualityComparer : IEqualityComparer<RuntimeTypeHandle>
    {
        /// <summary>
        /// Gets the default <see cref="RuntimeTypeHandleEqualityComparer"/>.
        /// </summary>
        /// <value>
        /// The default <see cref="RuntimeTypeHandleEqualityComparer"/>.
        /// </value>
        public static RuntimeTypeHandleEqualityComparer Default { get; } = new RuntimeTypeHandleEqualityComparer();

        /// <summary>
        /// Prevents a default instance of the <see cref="RuntimeTypeHandleEqualityComparer"/> class from being created.
        /// </summary>
        private RuntimeTypeHandleEqualityComparer()
        {

        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <see cref="RuntimeTypeHandle"/> to compare.</param>
        /// <param name="y">The second object of type <see cref="RuntimeTypeHandle"/> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(RuntimeTypeHandle x, RuntimeTypeHandle y) => x.Equals(y);

        /// <summary>
        /// Returns a hash code for the specified <see cref="RuntimeTypeHandle" />.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A hash code for the <see cref="RuntimeTypeHandle" />, suitable for use in hashing algorithms
        /// and data structures like a hash table.
        /// </returns>
        public int GetHashCode(RuntimeTypeHandle obj) => obj.GetHashCode();
    }
}
