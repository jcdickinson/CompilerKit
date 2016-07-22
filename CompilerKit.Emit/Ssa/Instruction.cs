using System;
using System.Collections.Generic;
using System.Globalization;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a single SSA instruction.
    /// </summary>
    public abstract class Instruction
    {
        /// <summary>
        /// Gets the list of input variables.
        /// </summary>
        /// <value>
        /// The list of input variables.
        /// </value>
        public abstract IReadOnlyList<Variable> InputVariables { get; }

        /// <summary>
        /// Gets list of the output variables.
        /// </summary>
        /// <value>
        /// The list of output variables.
        /// </value>
        public abstract IReadOnlyList<Variable> OutputVariables { get; }

        /// <summary>
        /// Gets the block that the instruction belongs to.
        /// </summary>
        /// <value>
        /// The block that the instruction belongs to.
        /// </value>
        public Block Block { get; internal set; }

        /// <summary>
        /// Gets the index of the instruction as it appears in a body.
        /// </summary>
        /// <value>
        /// The index of the instruction as it appears in a body.
        /// </value>
        public int Index { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Instruction"/> class.
        /// </summary>
        protected Instruction()
        {
            Index = -1;
        }

        /// <summary>
        /// Marks the specified <see cref="Variable"/> as assigned by this <see cref="Instruction"/>.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <returns>The value of <paramref name="variable"/>.</returns>
        /// <exception cref="System.InvalidOperationException">The variable has already been assigned.</exception>
        protected Variable Assign(Variable variable)
        {
            if (variable == null) return null;
            if (variable.AssignedBy != null) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.InvalidOperation_VariableAssigned, variable));
            variable.AssignedBy = this;
            return variable;
        }

        /// <summary>
        /// Compiles the method to the specified <see cref="ILGenerator" />.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator" /> to compile to.</param>
        /// 
        public abstract void CompileTo(IILGenerator il);
    }
}
