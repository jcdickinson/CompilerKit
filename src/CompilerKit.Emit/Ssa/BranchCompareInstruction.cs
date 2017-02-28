using System.Collections.Immutable;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents an instruction that branches to a single location
    /// based on the result of a comparison.
    /// </summary>
    public class BranchCompareInstruction : BranchInstruction
    {
        /// <summary>
        /// Gets the comparison operation that must pass for the
        /// branch to occur.
        /// </summary>
        /// <value>
        /// The comparison operation that must pass for the
        /// branch to occur.
        /// </value>
        public Comparison Comparison { get; private set; }

        /// <summary>
        /// Gets the possible destination of the branch.
        /// </summary>
        /// <value>
        /// The possible destination of the branch.
        /// </value>
        public Block Destination { get; private set; }

        /// <summary>
        /// Gets the <see cref="Variable"/> that participates in the comparison on the left-hand side.
        /// </summary>
        /// <value>
        /// The <see cref="Variable"/> that participates in the comparison on the left-hand side.
        /// </value>
        public Variable Left { get; private set; }

        /// <summary>
        /// Gets the <see cref="Variable"/> that participates in the comparison on the right-hand side.
        /// </summary>
        /// <value>
        /// The <see cref="Variable"/> that participates in the comparison on the right-hand side.
        /// </value>
        public Variable Right { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="BranchCompareInstruction"/> is ordered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ordered; otherwise, <c>false</c>.
        /// </value>
        public bool Ordered { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchCompareInstruction"/> class.
        /// </summary>
        protected internal BranchCompareInstruction() { }

        /// <summary>
        /// Initializes an allocated instance.
        /// </summary>
        /// <param name="destination">The destination that will be jumped to.</param>
        /// <param name="left">The <see cref="Variable" /> that participates in the comparison on the left-hand side.</param>
        /// <param name="comparison">The dual-valued comparison that will be made.</param>
        /// <param name="right">The <see cref="Variable" /> that participates in the comparison on the right-hand side.</param>
        /// <param name="ordered">If <c>true</c>, the comparison will be ordered.</param>
        /// <returns>
        /// The <see cref="BranchCompareInstruction" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="comparison" /> is not one of the dual-valued comparisons.</exception>
        protected internal BranchCompareInstruction Allocate(Block destination, Variable left, Comparison comparison, Variable right, bool? ordered)
        {
            Destination = destination;
            Destinations = new[] { destination }.ToImmutableArray();
            OutputVariables = Variable.ImmutableEmptyVariables;
            Left = left;
            Right = right;
            Comparison = comparison;

            if (left == null && right == null)
            {
                InputVariables = Variable.ImmutableEmptyVariables;
                Ordered = true;
            }
            else if (right == null)
            {
                InputVariables = new[] { left }.ToImmutableArray();
                Ordered = left.IsSigned;
            }
            else
            {
                InputVariables = new[] { left, right }.ToImmutableArray();
                Ordered = left.IsSigned || right.IsSigned;
            }

            if (ordered.HasValue)
            {
                Ordered = ordered.Value;
            }

            return this;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> if the method is being called from <see cref="Dispose()"/>;
        /// <c>false</c> if it is being called from a finalizer.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                Destination = null;
                Destinations = Block.ImmutableEmptyBlocks;
                InputVariables = Variable.ImmutableEmptyVariables;
                Left = Right = null;
                Comparison = Comparison.Always;
            }
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>
        /// A value indicating whether this instance was returned to the pool.
        /// </returns>
        public override bool Free() => SsaFactory.Pools.BranchCompareInstruction.Free(this);

        /// <summary>
        /// Compiles the method to the specified <see cref="ILGenerator" />.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator" /> to compile to.</param>
        /// 
        public override void CompileTo(IILGenerator il)
        {
            if (!ReferenceEquals(Left, null)) il.Load(Left, EmitOptions.None);
            if (!ReferenceEquals(Right, null)) il.Load(Right, EmitOptions.None);
            il.Branch(Comparison, Destination, Ordered ? EmitOptions.SignedOrOrdered : EmitOptions.None);
        }
    }
}
