using System;
using System.Collections.Generic;

namespace CompilerKit.Collections.Generic
{
    /// <summary>
    /// Represents a dependency tree.
    /// </summary>
    /// <typeparam name="T">The type of the node in the dependency tree.</typeparam>
    public interface IDependencyTree<T> : IEnumerable<Tuple<T, IEnumerable<T>>>
    {
        /// <summary>
        /// Adds a new dependency.
        /// </summary>
        /// <param name="dependant">The dependant.</param>
        /// <param name="dependencies">The dependencies.</param>
        void Add(T dependant, IEnumerable<T> dependencies);

        /// <summary>
        /// Resolves the cyclic dependencies in the tree.
        /// </summary>
        /// <returns>The reverse-topographically sorted strongly connected components in the tree.</returns>
        IEnumerable<IReadOnlyList<T>> ResolveDependencies();
    }
}
