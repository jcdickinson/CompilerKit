using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CompilerKit.Collections.Generic
{
    /// <summary>
    /// Represents a dependency graph.
    /// </summary>
    /// <typeparam name="T">The type of the node in the dependency graph.</typeparam>
    public sealed class DependencyGraph<T> : IDependencyGraph<T>
    {
        private sealed class Box<TBoxed>
        {
            [ThreadStatic]
            private static Box<TBoxed> _instance;
            public static Box<TBoxed> Instance => (ReferenceEquals(_instance, null)) ? _instance = new Box<TBoxed>() : _instance;

            public TBoxed Value;

            public Box() { }

            public Box(TBoxed value) { Value = value; }

            public static implicit operator TBoxed(Box<TBoxed> box) => ReferenceEquals(box, null) ? default(TBoxed) : box.Value;

            public static implicit operator Box<TBoxed>(TBoxed value) => ReferenceEquals(value, default(TBoxed)) ? default(TBoxed) : value;
        }

        private sealed class Node : IEquatable<Node>
        {
            public readonly T Dependant;
            public readonly HashSet<Node> Dependencies;
            public int Index;
            public int LowLink;
            public bool OnStack;

            public Node(T dependant, IEqualityComparer<T> equalityComparer)
            {
                Dependant = dependant;
                Dependencies = new HashSet<Node>();
                Index = -1;
                LowLink = -1;
            }

            public override int GetHashCode()
            {
                return Dependant.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return Equals((Node)obj);
            }

            public bool Equals(Node other)
            {
                if (ReferenceEquals(other, null)) return false;
                return Dependant.Equals(other.Dependant);
            }
        }

        private readonly IEqualityComparer<T> _equalityComparer;
        private readonly Dictionary<T, Node> _nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyGraph{T}"/> class.
        /// </summary>
        public DependencyGraph() : this(EqualityComparer<T>.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyGraph{T}"/> class.
        /// </summary>
        public DependencyGraph(IEqualityComparer<T> equalityComparer)
        {
            _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
            _nodes = new Dictionary<T, Node>(_equalityComparer);
        }

        private Node GetNode(T value)
        {
            Node node;
            if (!_nodes.TryGetValue(value, out node))
                _nodes.Add(value, node = new Node(value, _equalityComparer));
            return node;
        }

        /// <summary>
        /// Adds a new dependency.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="dependencies">The nodes that depend on the node.</param>
        public void Add(T node, IEnumerable<T> dependencies)
        {
            lock (_nodes)
            {
                var dependantNode = GetNode(node);

                foreach (var dependency in dependencies)
                {
                    dependantNode.Dependencies.Add(GetNode(dependency));
                }
            }
        }

        /// <summary>
        /// Adds a new dependency.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="dependencies">The dependency.</param>
        public void Add(IEnumerable<T> nodes, T dependency)
        {
            lock (_nodes)
            {
                var dependencyNode = GetNode(dependency);

                foreach (var node in nodes)
                {
                    GetNode(node).Dependencies.Add(dependencyNode);
                }
            }
        }

        /// <summary>
        /// Adds a new dependency.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="dependencies">The dependencies.</param>
        public void Add(IEnumerable<T> nodes, IEnumerable<T> dependencies)
        {
            lock (_nodes)
            {
                foreach (var node in nodes)
                {
                    var dependantNode = GetNode(node);

                    foreach (var dependency in dependencies)
                    {
                        dependantNode.Dependencies.Add(GetNode(dependency));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the dependencies of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The list of the node dependencies.</returns>
        public IEnumerable<T> GetDependencies(T node)
        {
            var deps = _nodes[node].Dependencies;
            if (deps.Count == 0) return Enumerable.Empty<T>();
            return deps.Select(x => x.Dependant);
        }

        /// <summary>
        /// Resolves the cyclic dependencies in the graph.
        /// </summary>
        /// <returns>The reverse-topographically sorted strongly connected components in the graph.</returns>
        public IEnumerable<IReadOnlyList<T>> ResolveDependencies()
        {
            lock (_nodes)
            {
                foreach (var node in _nodes.Values)
                    node.LowLink = node.Index = -1;

                // Grab a local copy, don't hit TLS all the time.
                var index = Box<int>.Instance;
                index.Value = 0;

                var stack = new Stack<Node>();
                foreach (var node in _nodes.Values)
                {
                    if (node.Index < 0)
                    {
                        foreach (var scc in StrongConnect(node, index, stack))
                            yield return scc;
                    }
                }
            }
        }

        // https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
        // Read up on DFS orderings to fully grok this algorithm: https://en.wikipedia.org/wiki/Tree_traversal#Pre-order
        // It's the most beautiful algorithm in existence, and is pretty useful too.

        private IEnumerable<IReadOnlyList<T>> StrongConnect(Node v, Box<int> index, Stack<Node> stack)
        {
            var component = new List<T>();
            v.LowLink = v.Index = index;
            v.OnStack = true;
            index.Value++;
            stack.Push(v);

            // Consider successors of v
            foreach (var w in v.Dependencies)
            {
                if (w.Index < 0)
                {
                    // Successor w has not yet been visited; recurse on it
                    foreach (var scc in StrongConnect(w, index, stack))
                        if (scc != null) yield return scc;

                    if (w.LowLink < v.LowLink) v.LowLink = w.LowLink;
                }
                else if (w.OnStack)
                {
                    // Successor w is in stack S and hence in the current SCC
                    if (w.Index < v.LowLink) v.LowLink = w.Index;
                }
            }

            // If v is a root node, pop the stack and generate an SCC
            if (v.LowLink == v.Index)
            {
                var scc = new T[stack.Count];
                var i = -1;

                while (stack.Count > 0)
                {
                    var w = stack.Pop();
                    w.OnStack = false;
                    scc[++i] = w.Dependant;
                    if (ReferenceEquals(w, v)) break;
                }

                yield return new ArraySegment<T>(scc, 0, i + 1);
            }
        }

        private IEnumerable<Tuple<T, IEnumerable<T>>> GetEnumerable()
        {
            lock (_nodes)
            {
                foreach (var kvp in _nodes.Values)
                    yield return Tuple.Create(kvp.Dependant, kvp.Dependencies.Select(x => x.Dependant));
            }
        }

        public IEnumerator<Tuple<T, IEnumerable<T>>> GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
