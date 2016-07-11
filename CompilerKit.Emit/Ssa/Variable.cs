﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents an instance of a <see cref="RootVariable"/>.
    /// </summary>
    [DebuggerDisplay("{RootVariable}_{Subscript}")]
    public sealed class Variable : IEquatable<Variable>
    {
        private static readonly HashSet<RuntimeTypeHandle> _realTypes = new HashSet<RuntimeTypeHandle>()
        {
                typeof(float).TypeHandle,
                typeof(double).TypeHandle,
                typeof(decimal).TypeHandle
        };

        private static readonly HashSet<RuntimeTypeHandle> _integralTyes = new HashSet<RuntimeTypeHandle>()
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

        private static readonly HashSet<RuntimeTypeHandle> _signedTypes = new HashSet<RuntimeTypeHandle>()
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
        /// Gets the root variable underlying this variable.
        /// </summary>
        /// <value>
        /// The root variable.
        /// </value>
        public RootVariable RootVariable { get; }

        /// <summary>
        /// Gets the subscript of the variable.
        /// </summary>
        /// <value>
        /// The subscript of the variable.
        /// </value>
        public int Subscript { get; }

        /// <summary>
        /// Gets the instruction that assigns a value to this variable.
        /// </summary>
        /// <value>
        /// The instruction that assigns a value to this variable.
        /// </value>
        public Instruction AssignedBy { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this variable is backed by a parameter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this variable is backed by a parameter; otherwise, <c>false</c>.
        /// </value>
        public bool IsParameter { get { return RootVariable.IsParameter; } }

        /// <summary>
        /// Gets the type of the variable.
        /// </summary>
        /// <value>
        /// The type of the variable.
        /// </value>
        public Type Type { get { return RootVariable.Type; } }

        /// <summary>
        /// Gets or sets the <see cref="VariableOptions"/>.
        /// </summary>
        /// <value>
        /// The <see cref="VariableOptions"/>.
        /// </value>
        public VariableOptions Options { get; set; }

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
        /// Gets a value indicating whether this variable's type is a value type.
        /// </summary>
        /// <value>
        /// <c>true</c> if the type is a value type; otherwise, <c>false</c>.
        /// </value>
        public bool IsValueType { get { return Type.IsValueType; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable" /> class.
        /// </summary>
        /// <param name="rootVariable">The root variable.</param>
        /// <param name="subscript">The subscript.</param>
        /// <param name="isParameter">if set to <c>true</c> the variable is a parameter.</param>
        internal Variable(RootVariable rootVariable, int subscript)
        {
            RootVariable = rootVariable;
            Subscript = subscript;
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
            if ((ln ^ rn) | ln) return false;
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
            if ((ln ^ rn) | ln) return true;
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines if the specified <see cref="Variable"/>, is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Variable"/> to compare to this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="Variable" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Variable other)
        {
            if (ReferenceEquals(other, null)) return false;
            return RootVariable.Equals(other.RootVariable) &&
                   other.Subscript == Subscript;
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
            return Equals(obj as Variable);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return RootVariable.GetHashCode() * Subscript;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            const string VariableFormat = "{0}_{1}";
            return string.Format(CultureInfo.CurrentCulture, VariableFormat, RootVariable, Subscript);
        }
    }
}
