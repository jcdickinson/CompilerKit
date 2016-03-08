﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Emit;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a <see cref="IILGenerator"/> that emits to a
    /// <see cref="ILGenerator"/>.
    /// </summary>
    public class DefaultILGenerator : IILGenerator
    {
        /// <summary>
        /// Gets the <see cref="IReadOnlyDictionary{Variable, LocalBuilder}"/> that can be used
        /// to determine which <see cref="LocalBuilder"/> represents a <see cref="Variable"/>.
        /// </summary>
        /// <value>
        /// The variables dictionary.
        /// </value>
        protected IReadOnlyDictionary<Variable, LocalBuilder> Variables { get; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyDictionary{Block, Label}"/> that can be used
        /// to determine which <see cref="Label"/> represents a <see cref="Block"/>.
        /// </summary>
        /// <value>
        /// The variables dictionary.
        /// </value>
        protected IReadOnlyDictionary<Block, Label> Blocks { get; }

        /// <summary>
        /// Gets the target <see cref="ILGenerator"/>.
        /// </summary>
        /// <value>
        /// The target <see cref="ILGenerator"/>.
        /// </value>
        protected ILGenerator IL { get; }

        private readonly Dictionary<Variable, LocalBuilder> _variables;
        private readonly Dictionary<Block, Label> _blocks;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultILGenerator" /> class.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator"/>.</param>
        public DefaultILGenerator(ILGenerator il)
        {
            if (il == null) throw new ArgumentNullException(nameof(il));
            IL = il;
            _variables = new Dictionary<Variable, LocalBuilder>();
            _blocks = new Dictionary<Block, Label>();

            Variables = new ReadOnlyDictionary<Variable, LocalBuilder>(_variables);
            Blocks = new ReadOnlyDictionary<Block, Label>(_blocks);
        }

        /// <summary>
        /// Emits a binary operator.
        /// </summary>
        /// <param name="binaryOperator">The binary operator.</param>
        /// <param name="options">The options that describe how the operator should behave.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual void Binary(BinaryOperator binaryOperator, EmitOptions options)
        {
            var ovf = (options & EmitOptions.Checked) == EmitOptions.Checked;
            var un = (options & EmitOptions.SignedOrOrdered) != EmitOptions.SignedOrOrdered;
            var ovfUn = ovf | un;

            var not = false;
            switch (binaryOperator)
            {
                case BinaryOperator.Add:
                    if (ovfUn) IL.Emit(OpCodes.Add_Ovf_Un);
                    else if (ovf) IL.Emit(OpCodes.Add_Ovf);
                    else IL.Emit(OpCodes.Add);
                    break;
                case BinaryOperator.Subtract:
                    if (ovfUn) IL.Emit(OpCodes.Sub_Ovf_Un);
                    else if (ovf) IL.Emit(OpCodes.Sub_Ovf);
                    else IL.Emit(OpCodes.Sub);
                    break;
                case BinaryOperator.Multiply:
                    if (ovfUn) IL.Emit(OpCodes.Mul_Ovf_Un);
                    else if (ovf) IL.Emit(OpCodes.Mul_Ovf);
                    else IL.Emit(OpCodes.Mul);
                    break;
                case BinaryOperator.Divide:
                    if (un) IL.Emit(OpCodes.Div_Un);
                    else IL.Emit(OpCodes.Div);
                    break;
                case BinaryOperator.Modulo:
                    if (un) IL.Emit(OpCodes.Rem_Un);
                    else IL.Emit(OpCodes.Rem);
                    break;
                case BinaryOperator.Equal: IL.Emit(OpCodes.Ceq); break;
                case BinaryOperator.NotEqual:
                    IL.Emit(OpCodes.Ceq);
                    not = true;
                    break;
                case BinaryOperator.LessThan:
                    if (un) IL.Emit(OpCodes.Clt_Un);
                    else IL.Emit(OpCodes.Clt);
                    break;
                case BinaryOperator.LessThanOrEqual:
                    if (un) IL.Emit(OpCodes.Cgt_Un);
                    else IL.Emit(OpCodes.Cgt);
                    not = true;
                    break;
                case BinaryOperator.GreaterThan:
                    if (un) IL.Emit(OpCodes.Cgt_Un);
                    else IL.Emit(OpCodes.Cgt);
                    break;
                case BinaryOperator.GreaterThanOrEqual:
                    if (un) IL.Emit(OpCodes.Clt_Un);
                    else IL.Emit(OpCodes.Clt);
                    not = true;
                    break;
                case BinaryOperator.LeftShift: IL.Emit(OpCodes.Shl); break;
                case BinaryOperator.RightShift:
                    if (un) IL.Emit(OpCodes.Shr_Un);
                    else IL.Emit(OpCodes.Shr);
                    break;
                case BinaryOperator.And: IL.Emit(OpCodes.And); break;
                case BinaryOperator.Or: IL.Emit(OpCodes.Or); break;
                case BinaryOperator.ExclusiveOr: IL.Emit(OpCodes.Xor); break;
                default: throw new NotSupportedException();
            }

            if (not)
            {
                IL.Emit(OpCodes.Ldc_I4_0);
                IL.Emit(OpCodes.Ceq);
            }
        }

        /// <summary>
        /// Emits the start of the specified body.
        /// </summary>
        /// <param name="block">The block to declare.</param>
        public virtual void Declare(Block block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            _blocks.Add(block, IL.DefineLabel());
        }

        /// <summary>
        /// Emits a declaration of the specified variable.
        /// </summary>
        /// <param name="variable">The variable to declare.</param>
        public virtual void Declare(Variable variable)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            var local = IL.DeclareLocal(variable.Type);
            _variables.Add(variable, local);
        }

        /// <summary>
        /// Emits the preamble for the specified block.
        /// </summary>
        /// <param name="block">The block to emit.</param>
        public virtual void Emit(Block block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            IL.MarkLabel(_blocks[block]);
        }

        /// <summary>
        /// Emits an instruction that loads a stack value from the specified <see cref="Variable" />.
        /// </summary>
        /// <param name="variable">The variable to load from.</param>
        /// <param name="options">The options that describe how the <see cref="Variable" /> should be loaded.</param>
        public virtual void Load(Variable variable, EmitOptions options)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));

            LocalBuilder local;
            int index;
            if (_variables.TryGetValue(variable, out local))
            {
                index = local.LocalIndex;
            }
            else
            {
                if (variable.IsParameter)
                    index = variable.RootVariable.Index;
                else
                    throw new ArgumentOutOfRangeException(nameof(variable));
            }

            if (variable.IsParameter)
            {
                switch (index)
                {
                    case 0: IL.Emit(OpCodes.Ldarg_0); break;
                    case 1: IL.Emit(OpCodes.Ldarg_1); break;
                    case 2: IL.Emit(OpCodes.Ldarg_2); break;
                    case 3: IL.Emit(OpCodes.Ldarg_3); break;
                    default:
                        if (index < 255)
                            IL.Emit(OpCodes.Ldarg_S, (byte)index);
                        else
                            IL.Emit(OpCodes.Ldarg, index);
                        break;
                }
            }
            else
            {
                switch (index)
                {
                    case 0: IL.Emit(OpCodes.Ldloc_0); break;
                    case 1: IL.Emit(OpCodes.Ldloc_1); break;
                    case 2: IL.Emit(OpCodes.Ldloc_2); break;
                    case 3: IL.Emit(OpCodes.Ldloc_3); break;
                    default:
                        if (index < 255)
                            IL.Emit(OpCodes.Ldloc_S, (byte)index);
                        else
                            IL.Emit(OpCodes.Ldloc, index);
                        break;
                }
            }
        }

        /// <summary>
        /// Emits an instruction that returns from the method.
        /// </summary>
        public virtual void Return()
        {
            IL.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits an instruction that stores a stack value into the specified <see cref="Variable" />.
        /// </summary>
        /// <param name="variable">The variable to store into.</param>
        /// <param name="options">The options that describe how the <see cref="Variable" /> should be stored.</param>
        public virtual void Store(Variable variable, EmitOptions options)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));

            LocalBuilder local;
            int index;
            if (_variables.TryGetValue(variable, out local))
            {
                index = local.LocalIndex;
            }
            else
            {
                if (variable.IsParameter)
                    index = variable.RootVariable.Index;
                else
                    throw new ArgumentOutOfRangeException(nameof(variable));
            }

            if (variable.IsParameter)
            {
                throw new NotImplementedException();
            }
            else
            {
                switch (index)
                {
                    case 0: IL.Emit(OpCodes.Stloc_0); break;
                    case 1: IL.Emit(OpCodes.Stloc_1); break;
                    case 2: IL.Emit(OpCodes.Stloc_2); break;
                    case 3: IL.Emit(OpCodes.Stloc_3); break;
                    default:
                        if (index < 255)
                            IL.Emit(OpCodes.Stloc_S, (byte)index);
                        else
                            IL.Emit(OpCodes.Stloc, index);
                        break;
                }
            }
        }

        /// <summary>
        /// Emits a branch which will jump to the specified <see cref="Block" />
        /// based on the result of the specified <see cref="Comparison" />.
        /// </summary>
        /// <param name="comparison">The comparison that is used to determine if the branch should occur.</param>
        /// <param name="block">The block to jump to.</param>
        /// <param name="options">The options that describe how the comparison should behave.</param>
        public void Branch(Comparison comparison, Block block, EmitOptions options)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));

            var un = (options & EmitOptions.SignedOrOrdered) != EmitOptions.SignedOrOrdered;

            var label = _blocks[block];
            switch (comparison)
            {
                case Comparison.Always: IL.Emit(OpCodes.Br, label); break;
                case Comparison.True: IL.Emit(OpCodes.Brtrue, label); break;
                case Comparison.False: IL.Emit(OpCodes.Brfalse, label); break;
                case Comparison.Equal: IL.Emit(OpCodes.Beq, label); break;
                case Comparison.NotEqual:
                    if (un)
                    {
                        IL.Emit(OpCodes.Bne_Un);
                    }
                    else
                    {
                        // Strange.
                        IL.Emit(OpCodes.Ceq);
                        IL.Emit(OpCodes.Brfalse, label);
                    }
                    break;
                case Comparison.GreaterThan:
                    if (un) IL.Emit(OpCodes.Bgt);
                    else IL.Emit(OpCodes.Bgt_Un);
                    break;
                case Comparison.GreaterThanOrEqual:
                    if (un) IL.Emit(OpCodes.Bge);
                    else IL.Emit(OpCodes.Bge_Un);
                    break;
                case Comparison.LessThan:
                    if (un) IL.Emit(OpCodes.Blt);
                    else IL.Emit(OpCodes.Blt_Un);
                    break;
                case Comparison.LessThanOrEqual:
                    if (un) IL.Emit(OpCodes.Ble);
                    else IL.Emit(OpCodes.Ble_Un);
                    break;
                default: throw new NotSupportedException();
            }
        }
    }
}