using System;
using System.Collections.Immutable;
using System.Globalization;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a single SSA instruction.
    /// </summary>
    public abstract class Instruction : IPooledObject, IDisposable
    {
        /// <summary>
        /// Gets the list of input variables.
        /// </summary>
        /// <value>
        /// The list of input variables.
        /// </value>
        public ImmutableArray<Variable> InputVariables { get; protected set; }

        /// <summary>
        /// Gets list of the output variables.
        /// </summary>
        /// <value>
        /// The list of output variables.
        /// </value>
        public ImmutableArray<Variable> OutputVariables { get; protected set; }

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

        }

        /// <summary>
        /// Marks the specified <see cref="Variable"/> as assigned by this <see cref="Instruction"/>.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <returns>The value of <paramref name="variable"/>.</returns>
        /// <exception cref="InvalidOperationException">The variable has already been assigned.</exception>
        protected Variable Assign(Variable variable)
        {
            if (variable == null) return null;
            if (variable.AssignedBy != null) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Exceptions.InvalidOperation_VariableAssigned, variable));
            variable.AssignedBy = this;
            return variable;
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>A value indicating whether this instance was returned to the pool.</returns>
        public abstract bool Free();

        /// <summary>
        /// Compiles the method to the specified <see cref="IILGenerator" />.
        /// </summary>
        /// <param name="il">The <see cref="IILGenerator" /> to compile to.</param>
        public abstract void CompileTo(IILGenerator il);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> if the method is being called from <see cref="Dispose()"/>;
        /// <c>false</c> if it is being called from a finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Index = -1;
                Block = null;
                InputVariables = Variable.ImmutableEmptyVariables;
                OutputVariables = Variable.ImmutableEmptyVariables;
            }
        }
    }
}
