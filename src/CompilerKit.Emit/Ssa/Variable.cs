using CompilerKit.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a variable in the SSA graph.
    /// </summary>
    [DebuggerDisplay("{Type,nq} {Name,nq}")]
    public sealed class Variable : IEquatable<Variable>
    {
        private static readonly ObjectPool<Variable> _pool
            = new ObjectPool<Variable>(() => new Variable(), 256);

        private static readonly HashSet<RuntimeTypeHandle> _realTypes = new HashSet<RuntimeTypeHandle>(RuntimeTypeHandleEqualityComparer.Default)
        {
                typeof(float).TypeHandle,
                typeof(double).TypeHandle,
                typeof(decimal).TypeHandle
        };

        private static readonly HashSet<RuntimeTypeHandle> _integralTyes = new HashSet<RuntimeTypeHandle>(RuntimeTypeHandleEqualityComparer.Default)
        {
                typeof(bool).TypeHandle,
                typeof(char).TypeHandle,
                typeof(sbyte).TypeHandle,
                typeof(short).TypeHandle,
                typeof(int).TypeHandle,
                typeof(long).TypeHandle,

                typeof(byte).TypeHandle,
                typeof(ushort).TypeHandle,
                typeof(uint).TypeHandle,
                typeof(ulong).TypeHandle
        };

        private static readonly HashSet<RuntimeTypeHandle> _signedTypes = new HashSet<RuntimeTypeHandle>(RuntimeTypeHandleEqualityComparer.Default)
        {
                typeof(bool).TypeHandle,
                typeof(char).TypeHandle,
                typeof(sbyte).TypeHandle,
                typeof(short).TypeHandle,
                typeof(int).TypeHandle,
                typeof(long).TypeHandle,
                typeof(float).TypeHandle,
                typeof(double).TypeHandle,
                typeof(decimal).TypeHandle
        };

        /// <summary>
        /// Gets an array of variables that is empty.
        /// </summary>
        /// <value>
        /// The array of variables that is empty.
        /// </value>
        public static Variable[] EmptyVariables { get; } = new Variable[0];

        /// <summary>
        /// Gets an array of variables that is empty, as a <see cref="ImmutableArray{Variable}"/>.
        /// </summary>
        /// <value>
        /// The array of variables that is empty, as a <see cref="ImmutableArray{Variable}"/>.
        /// </value>
        public static ImmutableArray<Variable> ImmutableEmptyVariables { get; } = EmptyVariables.ToImmutableArray();

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this variable is backed by a parameter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this variable is backed by a parameter; otherwise, <c>false</c>.
        /// </value>
        public bool IsParameter { get; private set; }

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
        /// Gets the index of this variable within its collection.
        /// </summary>
        /// <value>
        /// The index of this variable within its collection.
        /// </value>
        public int Index { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this variable's type is integral.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this variable's type is integral; otherwise, <c>false</c>.
        /// </value>
        public bool IsIntegral { get { return _integralTyes.Contains(Type.TypeHandle); } }

        /// <summary>
        /// Gets a value indicating whether this variable's type is real.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this variable's type is real; otherwise, <c>false</c>.
        /// </value>
        public bool IsReal { get { return _realTypes.Contains(Type.TypeHandle); } }

        /// <summary>
        /// Gets a value indicating whether this variable's type is signed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the type is signed; otherwise, <c>false</c>.
        /// </value>
        public bool IsSigned { get { return _signedTypes.Contains(Type.TypeHandle); } }

        /// <summary>
        /// Gets the <see cref="Instruction"/> that assigns to this variable.
        /// </summary>
        /// <value>
        /// The <see cref="Instruction"/> that assigns to this variable.
        /// </value>
        public object AssignedBy { get; internal set; }

        private int _hashCode;

        /// <summary>
        /// Prevents a default instance of the <see cref="Variable"/> class from being created.
        /// </summary>
        internal Variable() { }

        /// <summary>
        /// Initializes an allocated instance.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <param name="typeInfo">The type information.</param>
        /// <param name="isParameter">If set to <c>true</c>, the variable is a parameter.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The <see cref="Variable" />.
        /// </returns>
        internal Variable Allocate(string name, Type type, TypeInfo typeInfo, bool isParameter, int index)
        {
            Name = name;
            Type = type;
            TypeInfo = typeInfo;
            IsParameter = isParameter;
            Index = index;
            _hashCode = StringComparer.Ordinal.GetHashCode(name);
            return this;
        }

        /// <summary>
        /// Frees this <see cref="Variable"/>.
        /// </summary>
        /// <returns>A value indicating whether this instance was returned to the pool.</returns>
        internal bool Free()
        {
            AssignedBy = null;
            return _pool.Free(this);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The first <see cref="Variable"/> to compare.</param>
        /// <param name="right">The second <see cref="Variable"/> to compare.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Variable left, Variable right)
        {
            var ln = ReferenceEquals(left, null);
            var rn = ReferenceEquals(right, null);
            if (ln && rn) return true;
            if (ln) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The first <see cref="Variable"/> to compare.</param>
        /// <param name="right">The second <see cref="Variable"/> to compare.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Variable left, Variable right)
        {
            var ln = ReferenceEquals(left, null);
            var rn = ReferenceEquals(right, null);
            if (ln && rn) return false;
            if (ln) return true;
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines if the specified <see cref="Variable"/>, is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Variable"/> to compare to this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="Variable" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Variable other) =>
            !ReferenceEquals(other, null) &&
            _hashCode == other._hashCode &&
            string.Equals(Name, other.Name, StringComparison.Ordinal);

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => Equals(obj as Variable);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => _hashCode;

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name;
    }
}
