using CompilerKit.Runtime;
using System;
using System.Diagnostics;
using System.Reflection;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a way to create unique variables.
    /// </summary>
    [DebuggerDisplay("{Type,nq} {Name,nq}_{_subscript,nq}")]
    public sealed class VariableFactory : IDisposable
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of the root variable.
        /// </summary>
        /// <value>
        /// The type of the root variable.
        /// </value>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets the type information.
        /// </summary>
        /// <value>
        /// The type information.
        /// </value>
        public TypeInfo TypeInfo { get; private set; }

        /// <summary>
        /// Gets the name of the next variable.
        /// </summary>
        /// <value>
        /// The name of the next variable.
        /// </value>
        internal string NextName
        {
            get
            {
                if (!_isAllocated) throw new ObjectDisposedException("VariableFactory");
                return $"{Name}_{_subscript++}";
            }
        }

        private int _subscript;
        private bool _isAllocated;

        /// <summary>
        /// Prevents a default instance of the <see cref="VariableFactory"/> class from being created.
        /// </summary>
        internal VariableFactory() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableFactory" /> struct.
        /// </summary>
        /// <param name="type">The type of the variable.</param>
        /// <param name="name">The name of the variable.</param>
        internal VariableFactory Allocate(Type type, string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (type == null) throw new ArgumentNullException(nameof(type));

            Name = name;
            Type = type;
            TypeInfo = type.GetTypeInfo();
            _subscript = 0;
            _isAllocated = true;

            return this;
        }

        /// <summary>
        /// Returns the <see cref="VariableFactory"/> to the internal pool.
        /// </summary>
        public void Dispose()
        {
            Name = null;
            Type = null;
            TypeInfo = null;

            if (_isAllocated)
            {
                _isAllocated = false;
                SsaFactory.Pools.VariableFactory.Free(this);
            }
        }
    }
}
