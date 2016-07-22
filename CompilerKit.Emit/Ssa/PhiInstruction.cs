using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CompilerKit.Emit.Ssa
{
    public class PhiInstruction : Instruction
    {
        /// <summary>
        /// Gets the list of input variables.
        /// </summary>
        /// <value>
        /// The list of input variables.
        /// </value>
        public override IReadOnlyList<Variable> InputVariables { get; }

        /// <summary>
        /// Gets list of the output variables.
        /// </summary>
        /// <value>
        /// The list of output variables.
        /// </value>
        public override IReadOnlyList<Variable> OutputVariables { get; }

        /// <summary>
        /// Gets the output variable.
        /// </summary>
        /// <value>
        /// The output variable.
        /// </value>
        public Variable Output { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhiInstruction"/> class.
        /// </summary>
        public PhiInstruction(Variable output, IList<Variable> inputVariables)
        {
            Output = output;
            OutputVariables = new ReadOnlyCollection<Variable>(new[] { output });
            InputVariables = new ReadOnlyCollection<Variable>(inputVariables);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhiInstruction"/> class.
        /// </summary>
        public PhiInstruction(Variable output, IEnumerable<Variable> inputVariables)
            : this(output, (inputVariables as IList<Variable>) ?? inputVariables.ToList())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhiInstruction"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="inputVariables">The input variables.</param>
        public PhiInstruction(Variable output, params Variable[] inputVariables)
            : this(output, (IEnumerable<Variable>)inputVariables)
        {

        }


        /// <summary>
        /// Compiles the method to the specified <see cref="ILGenerator" />.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator" /> to compile to.</param>
        public override void CompileTo(IILGenerator il)
        {

        }
    }
}
