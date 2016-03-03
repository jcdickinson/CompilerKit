using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection.Emit;

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
        /// Gets the body that the instruction belongs to.
        /// </summary>
        /// <value>
        /// The body that the instruction belongs to.
        /// </value>
        public Body Body { get; internal set; }

        /// <summary>
        /// Gets the list of locations where jumps may land.
        /// </summary>
        /// <value>
        /// The list of locations where jumps may land.
        /// </value>
        public IReadOnlyList<Instruction> JumpsTo { get; }

        /// <summary>
        /// Gets the list of locations where jumps originate to this instruction.
        /// </summary>
        /// <value>
        /// The  list of locations where jumps originate to this instruction.
        /// </value>
        public IReadOnlyList<Instruction> JumpsFrom { get; }

        /// <summary>
        /// Gets the index of the instruction as it appears in a body.
        /// </summary>
        /// <value>
        /// The index of the instruction as it appears in a body.
        /// </value>
        public int Index { get; internal set; }

        private readonly List<Instruction> _jumpsTo;
        private readonly List<Instruction> _jumpsFrom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Instruction"/> class.
        /// </summary>
        protected Instruction()
        {
            JumpsTo = new ReadOnlyCollection<Instruction>(_jumpsTo = new List<Instruction>(2));
            JumpsFrom = new ReadOnlyCollection<Instruction>(_jumpsFrom = new List<Instruction>(2));
            Index = -1;
        }

        protected void JumpTo(Instruction instruction)
        {
            instruction._jumpsFrom.Add(this);
            _jumpsTo.Add(instruction);
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
        /// Compiles the method to the specified <see cref="ILGenerator"/>.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator"/> to compile to.</param>
        public abstract void CompileTo(ILGenerator il);

        protected void EmitStore(ILGenerator il, Variable variable)
        {
            // NB: basically means *only* StackCandidate
            if (((variable.Options ^ VariableOptions.StackProhibited) & VariableOptions.StackOperations) ==
                VariableOptions.StackOperations)
            {

            }
            else if (variable.IsParameter)
            {
                throw new NotSupportedException();
            }
            else
            {
                var index = Body.Variables.IndexOf(variable);
                switch (index)
                {
                    case 0: il.Emit(OpCodes.Stloc_0); break;
                    case 1: il.Emit(OpCodes.Stloc_1); break;
                    case 2: il.Emit(OpCodes.Stloc_2); break;
                    case 3: il.Emit(OpCodes.Stloc_3); break;
                    default:
                        if (index < 255)
                            il.Emit(OpCodes.Stloc_S, (byte)index);
                        else
                            il.Emit(OpCodes.Stloc, index);
                        break;
                }
            }
        }

        protected void EmitLoad(ILGenerator il, Variable variable)
        {
            if (((variable.Options ^ VariableOptions.StackProhibited) & VariableOptions.StackOperations) ==
                VariableOptions.StackOperations)
            {

            }
            else if (variable.IsParameter)
            {
                var index = Body.Parameters.IndexOf(variable);
                switch (index)
                {
                    case 0: il.Emit(OpCodes.Ldarg_0); break;
                    case 1: il.Emit(OpCodes.Ldarg_1); break;
                    case 2: il.Emit(OpCodes.Ldarg_2); break;
                    case 3: il.Emit(OpCodes.Ldarg_3); break;
                    default:
                        if (index < 255)
                            il.Emit(OpCodes.Ldarg_S, (byte)index);
                        else
                            il.Emit(OpCodes.Ldarg, index);
                        break;
                }
            }
            else
            {
                var index = Body.Variables.IndexOf(variable);
                switch (index)
                {
                    case 0: il.Emit(OpCodes.Ldloc_0); break;
                    case 1: il.Emit(OpCodes.Ldloc_1); break;
                    case 2: il.Emit(OpCodes.Ldloc_2); break;
                    case 3: il.Emit(OpCodes.Ldloc_3); break;
                    default:
                        if (index < 255)
                            il.Emit(OpCodes.Ldloc_S, (byte)index);
                        else
                            il.Emit(OpCodes.Ldloc, index);
                        break;
                }
            }
        }
    }
}
