using System;
using System.Collections;
using System.Collections.Generic;

namespace CompilerKit.Collections.Generic
{
    /// <summary>
    /// Represents a way to identify sets within a graph.
    /// </summary>
    /// <typeparam name="T">The type of the element in the set.</typeparam>
    public sealed class DisjointSet<T> : IEnumerable<T>
    {
        internal sealed class DisjointNode
        {
            public DisjointNode Parent;
            public T Value;
            public uint Rank;

            public DisjointNode(T value)
            {
                Parent = this;
                Rank = 0;
                Value = value;
            }

            public DisjointNode Find()
            {
                if (Parent.Parent != Parent) Parent = Parent.Find();
                return Parent;
            }
        }

        /// <summary>
        /// Represents an enumerator for <see cref="DisjointSet{T}"/>.
        /// </summary>
        public struct DisjointSetEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<DisjointNode> _enumerator;

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            object IEnumerator.Current => Current;

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            public T Current => _enumerator.Current == null
                ? default(T)
                : _enumerator.Current.Find().Value;

            /// <summary>
            /// Initializes a <see cref="DisjointSetEnumerator"/>.
            /// </summary>
            /// <param name="enumerator">The underlying enumerator.</param>
            internal DisjointSetEnumerator(IEnumerator<DisjointNode> enumerator)
            {
                _enumerator = enumerator;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the enumerator was successfully advanced to the next element; 
            /// <c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                while (_enumerator.MoveNext())
                {
                    // Only representative nodes.
                    if (_enumerator.Current.Find().Parent == _enumerator.Current)
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element
            /// in the collection.
            /// </summary>
            public void Reset() => _enumerator.Reset();

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting
            /// unmanaged resources.
            /// </summary>
            public void Dispose() => _enumerator.Dispose();
        }

        public T this[T value] => _nodes[value].Find().Value;

        private readonly IEqualityComparer<T> _equalityComparer;
        private readonly Dictionary<T, DisjointNode> _nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisjointSet{T}"/> class.
        /// </summary>
        public DisjointSet() 
            : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisjointSet{T}"/> class.
        /// </summary>
        /// <param name="equalityComparer">The comparer to use for equality operations.</param>
        public DisjointSet(IEqualityComparer<T> equalityComparer)
        {
            _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            _nodes = new Dictionary<T, DisjointNode>(_equalityComparer);
        }

        /// <summary>
        /// Clears the set.
        /// </summary>
        public void Clear()
        {
            _nodes.Clear();
        }

        private DisjointNode GetNode(T value)
        {
            if (!_nodes.TryGetValue(value, out var node))
                _nodes.Add(value, node = new DisjointNode(value));
            return node;
        }

        /// <summary>
        /// Adds the specified edge to the disjoint set.
        /// </summary>
        /// <param name="x">The first node.</param>
        /// <param name="y">The second node.</param>
        public void Union(T x, T y)
        {
            var nx = GetNode(x).Find();
            var ny = GetNode(y).Find();

            if (nx.Rank < ny.Rank)
                nx.Parent = ny;
            else
                ny.Parent = nx;

            if (nx.Rank == ny.Rank)
                nx.Rank++;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public DisjointSetEnumerator GetEnumerator() => new DisjointSetEnumerator(_nodes.Values.GetEnumerator());

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
