using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a constant instruction.
    /// </summary>
    public class ConstantInstruction : Instruction
    {
        /// <summary>
        /// Gets the value of the constant.
        /// </summary>
        /// <value>
        /// The value of the constant.
        /// </value>
        public object Value { get; }

        /// <summary>
        /// Gets the output variable.
        /// </summary>
        /// <value>
        /// The output variable.
        /// </value>
        public Variable Output { get; }

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
        /// Initializes a new instance of the <see cref="ConstantInstruction"/> class with
        /// the default value for the variable.
        /// </summary>
        public ConstantInstruction(Variable output)
            : this(output, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantInstruction" /> class with the
        /// specified value for the variable.
        /// </summary>
        /// <param name="value">The value of the constant.</param>
        public ConstantInstruction(Variable output, object value)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            Value = value;
            Output = output;
            InputVariables = new ReadOnlyCollection<Variable>(new[] { output });
            OutputVariables = new ReadOnlyCollection<Variable>(Variable.EmptyVariables);
        }

        public override void CompileTo(IILGenerator il)
        {
            if (ReferenceEquals(Value, null))
            {
                if (Output.IsValueType)
                {
                    throw new NotSupportedException();
                }
            }

            il.Constant(Value);
            il.Store(Output, EmitOptions.None);
        }
    }
}
