using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a root variable.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public sealed class RootVariable : IEquatable<RootVariable>
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the list of variables that are subscripts of this
        /// root variable.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IReadOnlyList<Variable> Variables { get; }

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
        public Type Type { get; }

        private readonly List<Variable> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootVariable"/> class.
        /// </summary>
        internal RootVariable(string name, Type type, bool isParameter)
        {
            Name = name;
            _variables = new List<Variable>();
            Variables = new ReadOnlyCollection<Variable>(_variables);
            IsParameter = isParameter;
            Type = type;
        }

        /// <summary>
        /// Gets the next the variable from the root variable.
        /// </summary>
        /// <returns>The next <see cref="Variable"/> instance.</returns>
        public Variable NextVariable()
        {
            var variable = new Variable(this, _variables.Count);
            _variables.Add(variable);
            return variable;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The first <see cref="RootVariable"/> to compare.</param>
        /// <param name="right">The second <see cref="RootVariable"/> to compare.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(RootVariable left, RootVariable right)
        {
            var ln = ReferenceEquals(left, null);
            var rn = ReferenceEquals(right, null);
            if ((ln ^ rn) | ln) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The first <see cref="RootVariable"/> to compare.</param>
        /// <param name="right">The second <see cref="RootVariable"/> to compare.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(RootVariable left, RootVariable right)
        {
            var ln = ReferenceEquals(left, null);
            var rn = ReferenceEquals(right, null);
            if ((ln ^ rn) | ln) return true;
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines if the specified <see cref="RootVariable"/>, is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="RootVariable"/> to compare to this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="RootVariable" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(RootVariable other)
        {
            if (ReferenceEquals(other, null)) return false;
            return string.Equals(Name, other.Name, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as RootVariable);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Name);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
