using System;
using System.Collections.Immutable;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a constant instruction with an unknown type.
    /// </summary>
    public abstract class ConstantInstruction : Instruction
    {
        /// <summary>
        /// Gets the value of the constant.
        /// </summary>
        /// <value>
        /// The value of the constant.
        /// </value>
        public abstract object ObjectValue { get; }

        /// <summary>
        /// Gets the output variable.
        /// </summary>
        /// <value>
        /// The output variable.
        /// </value>
        public Variable Output { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantInstruction{T}"/> class with
        /// the default value for the variable.
        /// </summary>
        protected internal ConstantInstruction() { }

        /// <summary>
        /// Initializes an allocated instance.
        /// </summary>
        /// <param name="output">The output variable.</param>
        /// <param name="value">The value of the constant.</param>
        protected internal abstract ConstantInstruction AllocateObject(Variable output, object value);

        /// <summary>
        /// Initializes an allocated instance.
        /// </summary>
        /// <param name="output">The output variable.</param>
        /// <exception cref="System.ArgumentNullException">output</exception>
        protected void Allocate(Variable output)
        {
            Output = output;
            InputVariables = Variable.ImmutableEmptyVariables;
            OutputVariables = new[] { Assign(output) }.ToImmutableArray();
        }
    }

    /// <summary>
    /// Represents a constant instruction.
    /// </summary>
    /// <typeparam name="T">The type of the constant.</typeparam>
    public class ConstantInstruction<T> : ConstantInstruction
    {
        /// <summary>
        /// Gets the value of the constant.
        /// </summary>
        /// <value>
        /// The value of the constant.
        /// </value>
        public override object ObjectValue => Value;

        /// <summary>
        /// Gets the value of the constant.
        /// </summary>
        /// <value>
        /// The value of the constant.
        /// </value>
        public T Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantInstruction{T}"/> class with
        /// the default value for the variable.
        /// </summary>
        protected internal ConstantInstruction() { }

        /// <summary>
        /// Initializes an allocated instance.
        /// </summary>
        /// <param name="output">The output variable.</param>
        /// <param name="value">The value of the constant.</param>
        /// <returns>
        /// The <see cref="ConstantInstruction" />.
        /// </returns>
        protected internal override ConstantInstruction AllocateObject(Variable output, object value) => Allocate(output, (T)value);

        /// <summary>
        /// Initializes an allocated instance.
        /// </summary>
        /// <param name="output">The output variable.</param>
        /// <param name="value">The value of the constant.</param>
        /// <returns>
        /// The <see cref="ConstantInstruction{T}" />.
        /// </returns>
        protected internal ConstantInstruction<T> Allocate(Variable output, T value)
        {
            Value = value;
            Allocate(output);
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
                Value = default(T);
            }
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>
        /// A value indicating whether this instance was returned to the pool.
        /// </returns>
        public override bool Free() => SsaFactory.Pools<T>.ConstantInstruction.Free(this);

        /// <summary>
        /// Compiles the method to the specified <see cref="IILGenerator" />.
        /// </summary>
        /// <param name="il">The <see cref="IILGenerator" /> to compile to.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void CompileTo(IILGenerator il)
        {
            if (ReferenceEquals(Value, null))
            {
                if (Output.TypeInfo.IsValueType)
                {
                    throw new NotSupportedException();
                }
            }

            il.Constant(Value);
            il.Store(Output, EmitOptions.None);
        }
    }
}
