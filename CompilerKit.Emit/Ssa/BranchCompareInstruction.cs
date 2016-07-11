using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents an instruction that branches to a single location
    /// based on the result of a comparison.
    /// </summary>
    public class BranchCompareInstruction : BranchInstruction
    {
        /// <summary>
        /// Gets the list of destinations that the instruction
        /// may branch to.
        /// </summary>
        /// <value>
        /// The list of destinations that the instruction
        /// may branch to.
        /// </value>
        public sealed override IReadOnlyList<Block> Destinations { get; }

        /// <summary>
        /// Gets the list of input variables.
        /// </summary>
        /// <value>
        /// The list of input variables.
        /// </value>
        public sealed override IReadOnlyList<Variable> InputVariables { get; }

        /// <summary>
        /// Gets list of the output variables.
        /// </summary>
        /// <value>
        /// The list of output variables.
        /// </value>
        public sealed override IReadOnlyList<Variable> OutputVariables { get; }

        /// <summary>
        /// Gets the comparison operation that must pass for the
        /// branch to occur.
        /// </summary>
        /// <value>
        /// The comparison operation that must pass for the
        /// branch to occur.
        /// </value>
        public Comparison Comparison { get; }

        /// <summary>
        /// Gets the possible destination of the branch.
        /// </summary>
        /// <value>
        /// The possible destination of the branch.
        /// </value>
        public Block Destination { get; }

        /// <summary>
        /// Gets the <see cref="Variable"/> that participates in the comparison on the left-hand side.
        /// </summary>
        /// <value>
        /// The <see cref="Variable"/> that participates in the comparison on the left-hand side.
        /// </value>
        public Variable Left { get; }

        /// <summary>
        /// Gets the <see cref="Variable"/> that participates in the comparison on the right-hand side.
        /// </summary>
        /// <value>
        /// The <see cref="Variable"/> that participates in the comparison on the right-hand side.
        /// </value>
        public Variable Right { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="BranchCompareInstruction"/> is ordered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ordered; otherwise, <c>false</c>.
        /// </value>
        public bool Ordered { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchCompareInstruction" /> class
        /// that will always branch to the specified destination.
        /// </summary>
        /// <param name="destination">The destination that will be jumped to.</param>
        public BranchCompareInstruction(Block destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            Destination = destination;
            Comparison = Comparison.Always;
            Destinations = new ReadOnlyCollection<Block>(new[] { destination });
            InputVariables = OutputVariables = new ReadOnlyCollection<Variable>(Variable.EmptyVariables);
            Ordered = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchCompareInstruction" /> class
        /// that will always branch to the specified destination.
        /// </summary>
        /// <param name="destination">The destination that will be jumped to.</param>
        /// <param name="value">The single value to compare.</param>
        /// <param name="comparison">The single-valued comparison that will be made.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="comparison" /> is not one of the single-valued comparisons.</exception>
        public BranchCompareInstruction(Block destination, Variable value, Comparison comparison)
            : this(destination)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            switch (comparison)
            {
                case Comparison.True:
                case Comparison.False:
                    break;
                default:
                    break; throw new ArgumentOutOfRangeException(nameof(comparison));
            }

            Comparison = comparison;
            Left = value;
            InputVariables = new ReadOnlyCollection<Variable>(new[] { value });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchCompareInstruction" /> class
        /// that will always branch to the specified destination.
        /// </summary>
        /// <param name="destination">The destination that will be jumped to.</param>
        /// <param name="left">The <see cref="Variable" /> that participates in the comparison on the left-hand side.</param>
        /// <param name="comparison">The dual-valued comparison that will be made.</param>
        /// <param name="right">The <see cref="Variable" /> that participates in the comparison on the right-hand side.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="comparison" /> is not one of the dual-valued comparisons.</exception>
        public BranchCompareInstruction(Block destination, Variable left, Comparison comparison, Variable right)
            : this(destination)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            switch (comparison)
            {
                case Comparison.Equal:
                case Comparison.NotEqual:
                case Comparison.GreaterThan:
                case Comparison.GreaterThanOrEqual:
                case Comparison.LessThan:
                case Comparison.LessThanOrEqual:
                    break;
                default:
                    break; throw new ArgumentOutOfRangeException(nameof(comparison));
            }

            Comparison = comparison;
            Left = left;
            Right = right;
            Ordered = left.IsSigned || right.IsSigned;
            InputVariables = new ReadOnlyCollection<Variable>(new[] { left, right });
        }

        /// <summary>
        /// Compiles the method to the specified <see cref="ILGenerator" />.
        /// </summary>
        /// <param name="emitRequest">The emit request.</param>
        /// <param name="il">The <see cref="ILGenerator" /> to compile to.</param>
        public override void CompileTo(IMethodEmitRequest emitRequest, IILGenerator il)
        {
            if (!ReferenceEquals(Left, null)) il.Load(Left, EmitOptions.None);
            if (!ReferenceEquals(Right, null)) il.Load(Right, EmitOptions.None);
            il.Branch(Comparison, Destination, Ordered ? EmitOptions.SignedOrOrdered : EmitOptions.None);
        }
    }
}
