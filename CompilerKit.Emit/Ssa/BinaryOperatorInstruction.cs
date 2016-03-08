using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection.Emit;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents an instruction that takes two input values
    /// and performs a mathematical operator on them.
    /// </summary>
    public class BinaryOperatorInstruction : Instruction
    {
        private static readonly Dictionary<Tuple<BinaryOperator, RuntimeTypeHandle, RuntimeTypeHandle>, Type> _operatorResults = new Dictionary<Tuple<BinaryOperator, RuntimeTypeHandle, RuntimeTypeHandle>, Type>()
        {
            // https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.add.aspx
            { Tuple.Create(BinaryOperator.Add, typeof(int).TypeHandle, typeof(int).TypeHandle), typeof(int) },
            { Tuple.Create(BinaryOperator.Add, typeof(long).TypeHandle, typeof(long).TypeHandle), typeof(long) },
            { Tuple.Create(BinaryOperator.Add, typeof(float).TypeHandle, typeof(float).TypeHandle), typeof(float) },
            { Tuple.Create(BinaryOperator.Add, typeof(double).TypeHandle, typeof(double).TypeHandle), typeof(double) },

            { Tuple.Create(BinaryOperator.Equal, typeof(int).TypeHandle, typeof(int).TypeHandle), typeof(bool) },
            { Tuple.Create(BinaryOperator.Equal, typeof(long).TypeHandle, typeof(long).TypeHandle), typeof(bool) },
            { Tuple.Create(BinaryOperator.Equal, typeof(float).TypeHandle, typeof(float).TypeHandle), typeof(bool) },
            { Tuple.Create(BinaryOperator.Equal, typeof(double).TypeHandle, typeof(double).TypeHandle), typeof(bool) },

            { Tuple.Create(BinaryOperator.LeftShift, typeof(int).TypeHandle, typeof(int).TypeHandle), typeof(int) },
            { Tuple.Create(BinaryOperator.LeftShift, typeof(long).TypeHandle, typeof(int).TypeHandle), typeof(long) },

            { Tuple.Create(BinaryOperator.And, typeof(int).TypeHandle, typeof(int).TypeHandle), typeof(int) },
            { Tuple.Create(BinaryOperator.And, typeof(long).TypeHandle, typeof(long).TypeHandle), typeof(long) },
        };

        private static readonly HashSet<RuntimeTypeHandle> _signedTypes = new HashSet<RuntimeTypeHandle>()
        {
                typeof(bool).TypeHandle,
                typeof(char).TypeHandle,
                typeof(sbyte).TypeHandle,
                typeof(short).TypeHandle,
                typeof(int).TypeHandle,
                typeof(long).TypeHandle,
                typeof(float).TypeHandle,
                typeof(double).TypeHandle,
                typeof(decimal).TypeHandle,
        };

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
        /// Gets the output variable.
        /// </summary>
        /// <value>
        /// The output variable.
        /// </value>
        public Variable Output { get; }

        /// <summary>
        /// Gets the left variable.
        /// </summary>
        /// <value>
        /// The left variable.
        /// </value>
        public Variable Left { get; }

        /// <summary>
        /// Gets the binary operator.
        /// </summary>
        /// <value>
        /// The binary operator.
        /// </value>
        public BinaryOperator BinaryOperator { get; }

        /// <summary>
        /// Gets the right variable.
        /// </summary>
        /// <value>
        /// The right variable.
        /// </value>
        public Variable Right { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperatorInstruction" /> class.
        /// </summary>
        /// <param name="output">The root variable from which the output variable will be created.</param>
        /// <param name="left">The left variable that participated in the mathematical operation.</param>
        /// <param name="binaryOperator">The binary operator.</param>
        /// <param name="right">The right variable that participated in the mathematical operation.</param>
        public BinaryOperatorInstruction(Variable output, Variable left, BinaryOperator binaryOperator, Variable right)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            Output = Assign(output);
            Left = left;
            BinaryOperator = binaryOperator;
            Right = right;

            var type = GetBinaryOperatorResult(left.Type, binaryOperator, right.Type);
            if (!Unpun(type).Equals(Unpun(Output.Type)))
                throw new ArgumentOutOfRangeException(nameof(output), string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.InvalidOperation_VariableHasIncorrectType, output, type));

            InputVariables = new ReadOnlyCollection<Variable>(new[] { Left, Right });
            OutputVariables = new ReadOnlyCollection<Variable>(new[] { Output });
        }

        /// <summary>
        /// Compiles the method to the specified <see cref="ILGenerator" />.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator" /> to compile to.</param>
        public override void CompileTo(ILGenerator il)
        {
            EmitLoad(il, Left);
            EmitLoad(il, Right);
            switch (BinaryOperator)
            {
                case BinaryOperator.Add: il.Emit(OpCodes.Add); break;
                case BinaryOperator.Subtract: il.Emit(OpCodes.Sub); break;
                case BinaryOperator.Multiply: il.Emit(OpCodes.Mul); break;
                case BinaryOperator.Divide: il.Emit(OpCodes.Div); break;
                case BinaryOperator.Modulo: il.Emit(OpCodes.Rem); break;
                case BinaryOperator.Equal: il.Emit(OpCodes.Ceq); break;
                case BinaryOperator.NotEqual: il.Emit(OpCodes.Ceq); il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Ceq); break;
                case BinaryOperator.LessThan:
                    if (IsSigned(Left.Type.TypeHandle) && IsSigned(Right.Type.TypeHandle))
                        il.Emit(OpCodes.Clt);
                    else
                        il.Emit(OpCodes.Clt_Un);
                    break;
                case BinaryOperator.LessThanOrEqual:
                    if (IsSigned(Left.Type.TypeHandle) && IsSigned(Right.Type.TypeHandle))
                        il.Emit(OpCodes.Cgt);
                    else
                        il.Emit(OpCodes.Cgt_Un);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                case BinaryOperator.GreaterThan:
                    if (IsSigned(Left.Type.TypeHandle) && IsSigned(Right.Type.TypeHandle))
                        il.Emit(OpCodes.Cgt);
                    else
                        il.Emit(OpCodes.Cgt_Un);
                    break;
                case BinaryOperator.GreaterThanOrEqual:
                    if (IsSigned(Left.Type.TypeHandle) && IsSigned(Right.Type.TypeHandle))
                        il.Emit(OpCodes.Clt);
                    else
                        il.Emit(OpCodes.Clt_Un);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ceq);
                    break;
                case BinaryOperator.LeftShift: il.Emit(OpCodes.Shl); break;
                case BinaryOperator.RightShift:
                    if (IsSigned(Left.Type.TypeHandle))
                        il.Emit(OpCodes.Shr);
                    else
                        il.Emit(OpCodes.Shr_Un);
                    break;
                case BinaryOperator.And: il.Emit(OpCodes.And); break;
                case BinaryOperator.Or: il.Emit(OpCodes.Or); break;
                case BinaryOperator.ExclusiveOr: il.Emit(OpCodes.Xor); break;
                default: throw new NotSupportedException();
            }
            EmitStore(il, Output);
        }

        private static Type GetBinaryOperatorResult(Type left, BinaryOperator binaryOperator, Type right)
        {
            var key = Tuple.Create(Unpun(binaryOperator), Unpun(left), Unpun(right));
            Type result;
            if (!_operatorResults.TryGetValue(key, out result))
                throw new ArgumentOutOfRangeException(nameof(binaryOperator), string.Format(CultureInfo.CurrentCulture,
                    Properties.Resources.ArgumentOutOfRange_NoOperator, left, binaryOperator, right));
            return result;
        }

        private static bool IsSigned(RuntimeTypeHandle type)
        {
            return _signedTypes.Contains(type);
        }

        private static RuntimeTypeHandle Unpun(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32: return typeof(int).TypeHandle;
                case TypeCode.Int64:
                case TypeCode.UInt64: return typeof(long).TypeHandle;
                default: return type.TypeHandle;
            }
        }

        private static BinaryOperator Unpun(BinaryOperator binaryOperator)
        {
            switch (binaryOperator)
            {
                case BinaryOperator.Add:
                case BinaryOperator.Subtract:
                case BinaryOperator.Multiply:
                case BinaryOperator.Divide:
                case BinaryOperator.Modulo: return BinaryOperator.Add;
                case BinaryOperator.Equal:
                case BinaryOperator.NotEqual:
                case BinaryOperator.LessThan:
                case BinaryOperator.LessThanOrEqual:
                case BinaryOperator.GreaterThan:
                case BinaryOperator.GreaterThanOrEqual: return BinaryOperator.Equal;
                case BinaryOperator.LeftShift:
                case BinaryOperator.RightShift: return BinaryOperator.LeftShift;
                case BinaryOperator.And:
                case BinaryOperator.Or:
                case BinaryOperator.ExclusiveOr: return BinaryOperator.And;
                default: throw new ArgumentOutOfRangeException(nameof(binaryOperator));
            }
        }
    }
}
