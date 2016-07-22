using System;
using System.Collections.Generic;

namespace CompilerKit.Collections.Generic
{
    /// <summary>
    /// Represents a dependency graph.
    /// </summary>
    /// <typeparam name="T">The type of the node in the dependency graph.</typeparam>
    public interface IDependencyGraph<T> : IEnumerable<Tuple<T, IEnumerable<T>>>
    {
        /// <summary>
        /// Adds a new dependency.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="dependencies">The dependencies.</param>
        void Add(T node, IEnumerable<T> dependencies);

        /// <summary>
        /// Adds a new dependency.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="dependencies">The dependency.</param>
        void Add(IEnumerable<T> nodes, T dependency);

        /// <summary>
        /// Adds a new dependency.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="dependencies">The dependencies.</param>
        void Add(IEnumerable<T> nodes, IEnumerable<T> dependencies);

        /// <summary>
        /// Gets the dependencies of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The list of the node dependencies.</returns>
        IEnumerable<T> GetDependencies(T node);

        /// <summary>
        /// Resolves the cyclic dependencies in the graph.
        /// </summary>
        /// <returns>The reverse-topographically sorted strongly connected components in the graph.</returns>
        IEnumerable<IReadOnlyList<T>> ResolveDependencies();
    }
}
