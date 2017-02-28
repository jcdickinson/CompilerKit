using System.Collections.Immutable;

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
        public ImmutableArray<Block> Destinations { get; protected set; }
        
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
                Destinations = Block.ImmutableEmptyBlocks;
            }
        }
    }
}
