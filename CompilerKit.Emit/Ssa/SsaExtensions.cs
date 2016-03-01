using System;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents extension methods common to the Ssa namespace.
    /// </summary>
    public static class SsaExtensions
    {
        /// <summary>
        /// Declares a new variable with an automatic name.
        /// </summary>
        /// <typeparam name="T">The type of the variable.</typeparam>
        /// <param name="collection">The collection to add the variable to.</param>
        /// <returns>
        /// The declared variable.
        /// </returns>
        public static RootVariable Add<T>(this IRootVariableCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            return collection.Add(typeof(T));
        }

        /// <summary>
        /// Declares a new variable with the specified name.
        /// </summary>
        /// <param name="collection">The collection to add the variable to.</param>
        /// <param name="name">The name of the variable.</param>
        /// <returns>
        /// The declared variable.
        /// </returns>
        public static RootVariable Add<T>(this IRootVariableCollection collection, string name)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            return collection.Add(typeof(T), name);
        }
    }
}
