using CompilerKit.Runtime;
using System.Collections.Immutable;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents an instruction that returns a value from a method.
    /// </summary>
    public class ReturnInstruction : Instruction
    {
        /// <summary>
        /// Gets the return value.
        /// </summary>
        /// <value>
        /// The return value.
        /// </value>
        public Variable ReturnValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnInstruction"/> class.
        /// </summary>
        protected internal ReturnInstruction() { }

        /// <summary>
        /// Allocates a new instance of the <see cref="ReturnInstruction" /> class, with
        /// the specified return variable.
        /// </summary>
        /// <param name="returnValue">The return value.</param>
        /// <returns>The <see cref="ReturnInstruction"/>.</returns>
        protected internal ReturnInstruction Allocate(Variable returnValue)
        {
            ReturnValue = returnValue;
            InputVariables = returnValue == null
                ? Variable.ImmutableEmptyVariables
                : new[] { returnValue }.ToImmutableArray();
            OutputVariables = Variable.ImmutableEmptyVariables;

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
            if (disposing)
            {
                ReturnValue = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>
        /// A value indicating whether this instance was returned to the pool.
        /// </returns>
        public override bool Free() => SsaFactory.Pools.ReturnInstruction.Free(this);

        /// <summary>
        /// Compiles the method to the specified <see cref="IILGenerator" />.
        /// </summary>
        /// <param name="il">The <see cref="IILGenerator" /> to compile to.</param>
        public override void CompileTo(IILGenerator il)
        {
            if (ReturnValue != null)
                il.Load(ReturnValue, EmitOptions.None);
            il.Return();
        }
    }
}
