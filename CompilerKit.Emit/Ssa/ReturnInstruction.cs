using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents an instruction that returns a value from a method.
    /// </summary>
    public class ReturnInstruction : Instruction
    {
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
        /// Gets the return value.
        /// </summary>
        /// <value>
        /// The return value.
        /// </value>
        public Variable ReturnValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnInstruction"/> class.
        /// </summary>
        public ReturnInstruction()
            : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnInstruction" /> class.
        /// </summary>
        /// <param name="returnValue">The return value.</param>
        public ReturnInstruction(Variable returnValue)
        {
            ReturnValue = returnValue;
            InputVariables = returnValue == null
                ? new ReadOnlyCollection<Variable>(Variable.EmptyVariables)
                : new ReadOnlyCollection<Variable>(new[] { returnValue });

            OutputVariables = new ReadOnlyCollection<Variable>(Variable.EmptyVariables);
        }

        /// <summary>
        /// Compiles the method to the specified <see cref="ILGenerator" />.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator" /> to compile to.</param>
        public override void CompileTo(IMethodEmitRequest emitRequest, IILGenerator il)
        {
            if (ReturnValue != null)
                il.Load(ReturnValue, EmitOptions.None);
            il.Return();
        }
    }
}
