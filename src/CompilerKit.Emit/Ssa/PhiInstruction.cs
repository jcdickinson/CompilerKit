using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a Phi instruction.
    /// </summary>
    public class PhiInstruction : Instruction
    {
        /// <summary>
        /// Gets the output variable.
        /// </summary>
        public Variable Output { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhiInstruction"/> class.
        /// </summary>
        protected internal PhiInstruction()
        { }

        /// <summary>
        /// Initializes an allocated instance.
        /// </summary>
        /// <param name="output">The output variable.</param>
        /// <param name="value">The value of the constant.</param>
        /// <returns>
        /// The <see cref="PhiInstruction" />.
        /// </returns>
        internal PhiInstruction Allocate(Variable output, ImmutableArray<Variable> inputs)
        {
            OutputVariables = new[] { Output = Assign(output) }.ToImmutableArray();
            InputVariables = inputs;
            return this;
        }

        /// <summary>
        /// Compiles the method to the specified <see cref="IILGenerator" />.
        /// </summary>
        /// <param name="il">The <see cref="IILGenerator" /> to compile to.</param>
        public override void CompileTo(IILGenerator il)
        { }
        
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
                Output = null;
            }
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>A value indicating whether this instance was returned to the pool.</returns>
        public override bool Free() => SsaFactory.Pools.PhiInstruction.Free(this);
    }
}
