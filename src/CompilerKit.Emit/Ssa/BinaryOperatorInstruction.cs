using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;

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
        
        /// <summary>
        /// Gets the output variable.
        /// </summary>
        /// <value>
        /// The output variable.
        /// </value>
        public Variable Output { get; private set; }

        /// <summary>
        /// Gets the left variable.
        /// </summary>
        /// <value>
        /// The left variable.
        /// </value>
        public Variable Left { get; private set; }

        /// <summary>
        /// Gets the binary operator.
        /// </summary>
        /// <value>
        /// The binary operator.
        /// </value>
        public BinaryOperator BinaryOperator { get; private set; }

        /// <summary>
        /// Gets the right variable.
        /// </summary>
        /// <value>
        /// The right variable.
        /// </value>
        public Variable Right { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to perform an overflow check.
        /// </summary>
        /// <value>
        ///   <c>true</c> if an overflow check should be performed; otherwise, <c>false</c>.
        /// </value>
        public bool Checked { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="BinaryOperatorInstruction"/> is ordered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ordered; otherwise, <c>false</c>.
        /// </value>
        public bool Ordered { get; private set; }

        protected internal BinaryOperatorInstruction() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperatorInstruction" /> class.
        /// </summary>
        /// <param name="output">The root variable from which the output variable will be created.</param>
        /// <param name="left">The left variable that participated in the mathematical operation.</param>
        /// <param name="binaryOperator">The binary operator.</param>
        /// <param name="right">The right variable that participated in the mathematical operation.</param>
        /// <param name="isChecked">A value indicating whether to perform a checked operation.</param>
        /// <param name="isOrdered">A value indicating whether to perform an ordered operation.</param>
        protected internal BinaryOperatorInstruction Allocate(Variable output, Variable left, BinaryOperator binaryOperator, Variable right, bool isChecked, bool? isOrdered)
        {
            Output = Assign(output);
            BinaryOperator = binaryOperator;
            Left = left;
            Right = right;

            var type = GetBinaryOperatorResult(left.Type, binaryOperator, right.Type);
            if (!Unpun(type).Equals(Unpun(Output.Type)))
                throw new ArgumentOutOfRangeException(nameof(output), string.Format(CultureInfo.CurrentCulture,
                    Exceptions.InvalidOperation_VariableHasIncorrectType, output, type));

            InputVariables = new[] { Left, Right }.ToImmutableArray();
            OutputVariables = new[] { Output }.ToImmutableArray();

            Checked = isChecked;
            Ordered = isOrdered ?? left.IsSigned || right.IsSigned;

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
            base.Dispose(disposing);
            if (disposing)
            {
                Output = null;
                Left = null;
                Right = null;
                InputVariables = Variable.ImmutableEmptyVariables;
                OutputVariables = Variable.ImmutableEmptyVariables;
            }
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>A value indicating whether this instance was returned to the pool.</returns>
        public override bool Free() => SsaFactory.Pools.BinaryOperatorInstruction.Free(this);

        /// <summary>
        /// Compiles the method to the specified <see cref="ILGenerator" />.
        /// </summary>
        /// <param name="il">The <see cref="ILGenerator" /> to compile to.</param>
        /// 
        public override void CompileTo(IILGenerator il)
        {
            il.Load(Left, EmitOptions.None);
            il.Load(Right, EmitOptions.None);

            var options = Checked ? EmitOptions.Checked : EmitOptions.None;
            if ((Left.IsReal && Ordered) ||
                (Left.IsIntegral && Left.IsSigned && Right.IsSigned))
                options |= EmitOptions.SignedOrOrdered;

            il.Binary(BinaryOperator, options);
            il.Store(Output, EmitOptions.None);
        }

        private static Type GetBinaryOperatorResult(Type left, BinaryOperator binaryOperator, Type right)
        {
            var key = Tuple.Create(Unpun(binaryOperator), Unpun(left), Unpun(right));
            if (!_operatorResults.TryGetValue(key, out var result))
                throw new ArgumentOutOfRangeException(nameof(binaryOperator), string.Format(CultureInfo.CurrentCulture,
                    Exceptions.ArgumentOutOfRange_NoOperator, left, binaryOperator, right));
            return result;
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
