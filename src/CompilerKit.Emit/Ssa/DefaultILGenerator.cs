using CompilerKit.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a <see cref="IILGenerator" /> that emits to a
    /// <see cref="ILGenerator" />.
    /// </summary>
    public class DefaultILGenerator : IILGenerator, IPooledObject, IDisposable
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
        protected ILGenerator IL { get; private set; }

        /// <summary>
        /// Gets the <see cref="IBodyTarget"/> that this <see cref="ILGenerator"/>
        /// </summary>
        /// <value>
        /// The <see cref="IBodyTarget"/> that created this <see cref="IILGenerator"/>.
        /// </value>
        public IBodyTarget Target { get; private set; }

        private readonly Dictionary<Variable, LocalBuilder> _variables;
        private readonly Dictionary<Block, Label> _blocks;

        /// <summary>
        /// Prevents a default instance of the <see cref="DefaultILGenerator"/> class from being created.
        /// </summary>
        internal DefaultILGenerator()
        {
            _variables = new Dictionary<Variable, LocalBuilder>();
            _blocks = new Dictionary<Block, Label>();
            Variables = new ReadOnlyDictionary<Variable, LocalBuilder>(_variables);
            Blocks = new ReadOnlyDictionary<Block, Label>(_blocks);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultILGenerator" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="il">The <see cref="ILGenerator" />.</param>
        /// <returns>The <see cref="DefaultILGenerator"/>.</returns>
        internal DefaultILGenerator Allocate(IBodyTarget target, ILGenerator il)
        {
            IL = il;
            Target = target;

            return this;
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>A value indicating whether this instance was returned to the pool.</returns>
        public bool Free() => SsaFactory.Pools.DefaultILGenerator.Free(this);

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
            variable = Target.GetService<Services.IVariableService>().FindVariable(variable);

            int index;
            if (_variables.TryGetValue(variable, out var local))
            {
                index = local.LocalIndex;
            }
            else
            {
                if (variable.IsParameter)
                    index = variable.Index;
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
            variable = Target.GetService<Services.IVariableService>().FindVariable(variable);

            int index;
            if (_variables.TryGetValue(variable, out var local))
            {
                index = local.LocalIndex;
            }
            else
            {
                if (variable.IsParameter)
                    index = variable.Index;
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
                        IL.Emit(OpCodes.Bne_Un, label);
                    }
                    else
                    {
                        // Strange.
                        IL.Emit(OpCodes.Ceq);
                        IL.Emit(OpCodes.Brfalse, label);
                    }
                    break;
                case Comparison.GreaterThan:
                    if (un) IL.Emit(OpCodes.Bgt, label);
                    else IL.Emit(OpCodes.Bgt_Un, label);
                    break;
                case Comparison.GreaterThanOrEqual:
                    if (un) IL.Emit(OpCodes.Bge, label);
                    else IL.Emit(OpCodes.Bge_Un, label);
                    break;
                case Comparison.LessThan:
                    if (un) IL.Emit(OpCodes.Blt, label);
                    else IL.Emit(OpCodes.Blt_Un, label);
                    break;
                case Comparison.LessThanOrEqual:
                    if (un) IL.Emit(OpCodes.Ble, label);
                    else IL.Emit(OpCodes.Ble_Un, label);
                    break;
                default: throw new NotSupportedException();
            }
        }

        public void Constant<T>(T value)
        {
            if (value == null)
            {
                IL.Emit(OpCodes.Ldnull);
                return;
            }

            // typeof(X) == typeof(Y) results in a JIT-time check.
            if (typeof(T) == typeof(bool))
                Constant((bool)(object)value ? -1 : 0);
            else if (typeof(T) == typeof(char))
                Constant((int)(char)(object)value);
            else if (typeof(T) == typeof(sbyte))
                Constant((int)(sbyte)(object)value);
            else if (typeof(T) == typeof(byte))
                Constant((int)(byte)(object)value);
            else if (typeof(T) == typeof(short))
                Constant((int)(short)(object)value);
            else if (typeof(T) == typeof(ushort))
                Constant((int)(ushort)(object)value);
            else if (typeof(T) == typeof(int))
                Constant((int)(object)value);
            else if (typeof(T) == typeof(uint))
                Constant(unchecked((int)(uint)(object)value));
            else if (typeof(T) == typeof(long))
                Constant((long)(object)value);
            else if (typeof(T) == typeof(ulong))
                Constant(unchecked((long)(ulong)(object)value));
            else if (typeof(T) == typeof(float))
                Constant((float)(object)value);
            else if (typeof(T) == typeof(double))
                Constant((double)(object)value);
            else if (typeof(T) == typeof(string))
                Constant((string)(object)value);
            else
                ComplexConstant(value);
        }

        /// <summary>
        /// Emits a constant instruction for the specified <typeparam name="T"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ComplexConstant<T>(T value)
        {
            throw new NotSupportedException($"{typeof(T)} cannot be used as a constant.");
        }

        /// <summary>
        /// Emits a constant instruction for the specified <see cref="int"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Constant(int value)
        {
            switch (value)
            {
                case -1: IL.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: IL.Emit(OpCodes.Ldc_I4_0); break;
                case 1: IL.Emit(OpCodes.Ldc_I4_1); break;
                case 2: IL.Emit(OpCodes.Ldc_I4_2); break;
                case 3: IL.Emit(OpCodes.Ldc_I4_3); break;
                case 4: IL.Emit(OpCodes.Ldc_I4_4); break;
                case 5: IL.Emit(OpCodes.Ldc_I4_5); break;
                case 6: IL.Emit(OpCodes.Ldc_I4_6); break;
                case 7: IL.Emit(OpCodes.Ldc_I4_7); break;
                case 8: IL.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                        IL.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    else
                        IL.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        /// <summary>
        /// Emits a constant instruction for the specified <see cref="long"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Constant(long value)
        {
            IL.Emit(OpCodes.Ldc_I8, value);
        }

        /// <summary>
        /// Emits a constant instruction for the specified <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Constant(float value)
        {
            IL.Emit(OpCodes.Ldc_R4, value);
        }

        /// <summary>
        /// Emits a constant instruction for the specified <see cref="double"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Constant(double value)
        {
            IL.Emit(OpCodes.Ldc_R8, value);
        }

        /// <summary>
        /// Emits a constant instruction for the specified <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Constant(string value)
        {
            IL.Emit(OpCodes.Ldstr, value);
        }

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
                IL = null;
                Target = null;
                _variables.Clear();
                _blocks.Clear();
            }
        }
    }
}
