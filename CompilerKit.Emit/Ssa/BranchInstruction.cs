using System.Collections.Generic;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents an instruction that may branch to one or more
    /// locations.
    /// </summary>
    public abstract class BranchInstruction : Instruction
    {
        /// <summary>
        /// Gets the list of destinations that the instruction
        /// may branch to.
        /// </summary>
        /// <value>
        /// The list of destinations that the instruction
        /// may branch to.
        /// </value>
        public abstract IReadOnlyList<Block> Destinations { get; }
    }
}
