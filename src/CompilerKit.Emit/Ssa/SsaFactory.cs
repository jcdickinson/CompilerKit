using CompilerKit.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a factory for SSA types.
    /// </summary>
    public static class SsaFactory
    {
        #region Pools
        internal static class Pools<T>
        {
            public static readonly ObjectPool<ConstantInstruction<T>> ConstantInstruction =
                new ObjectPool<ConstantInstruction<T>>(() => new ConstantInstruction<T>(), 16);
        }

        internal static class Pools
        {
            public static readonly ObjectPool<ReturnInstruction> ReturnInstruction =
                new ObjectPool<ReturnInstruction>(() => new ReturnInstruction(), 128);

            public static readonly ObjectPool<BinaryOperatorInstruction> BinaryOperatorInstruction =
                new ObjectPool<BinaryOperatorInstruction>(() => new BinaryOperatorInstruction(), 128);

            public static readonly ObjectPool<BranchCompareInstruction> BranchCompareInstruction =
                new ObjectPool<BranchCompareInstruction>(() => new BranchCompareInstruction(), 8);

            public static readonly ObjectPool<PhiInstruction> PhiInstruction =
                new ObjectPool<Ssa.PhiInstruction>(() => new PhiInstruction(), 128);

            public static readonly ObjectPool<Variable> Variable
                = new ObjectPool<Variable>(() => new Variable(), 256);

            public static readonly ObjectPool<VariableFactory> VariableFactory =
                new ObjectPool<VariableFactory>(() => new VariableFactory(), 32);

            public static readonly ObjectPool<Block> Block =
                new ObjectPool<Block>(() => new Block(), 64);

            public static readonly ObjectPool<Body> Body
                = new ObjectPool<Body>(() => new Body(), 64);

            public static readonly ObjectPool<DefaultILGenerator> DefaultILGenerator =
                new ObjectPool<DefaultILGenerator>(() => new DefaultILGenerator(), 8);

            public static readonly ObjectPool<DynamicBodyTarget> DynamicBodyTarget =
                new ObjectPool<DynamicBodyTarget>(() => new DynamicBodyTarget());

            public static readonly ObjectPool<Services.RootVariableService> RootVariableService =
                new ObjectPool<Services.RootVariableService>(() => new Services.RootVariableService());

            public static readonly ObjectPool<Optimizers.VariableDestructionOptimizer.DisjointSetVariableService> DisjointSetVariableService =
                new ObjectPool<Optimizers.VariableDestructionOptimizer.DisjointSetVariableService>(() => new Optimizers.VariableDestructionOptimizer.DisjointSetVariableService());
        }
        #endregion

        #region Type Lookup
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, Func<object>> _genericAllocators =
            new ConcurrentDictionary<RuntimeTypeHandle, Func<object>>(RuntimeTypeHandleEqualityComparer.Default);
        private static T AllocateUnknownType<T>(RuntimeTypeHandle specialization)
        {
            if (_genericAllocators.TryGetValue(specialization, out var allocator))
                return (T)allocator();

            var baseType = typeof(T);
            var type = Type.GetTypeFromHandle(specialization);
            var name = baseType.Name;
            var poolType = typeof(Pools<>).MakeGenericType(type);
            var field = poolType.GetRuntimeField(name);
            var allocate = field.FieldType.GetRuntimeMethod("Allocate", Type.EmptyTypes);

            var access = Expression.MakeMemberAccess(null, field);
            var invoke = Expression.Call(access, allocate);
            var convert = Expression.Convert(invoke, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(convert);
            allocator = lambda.Compile(false);

            return (T)_genericAllocators.GetOrAdd(specialization, allocator)();
        }
        #endregion

        #region Instructions
        /// <summary>
        /// Allocates a new <see cref="Ssa.Variable" />.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <param name="typeInfo">The type information.</param>
        /// <param name="isParameter">If set to <c>true</c>, the variable is a parameter.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The <see cref="Variable" />.
        /// </returns>
        internal static Variable Variable(string name, Type type, TypeInfo typeInfo, bool isParameter, int index)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (typeInfo == null) throw new ArgumentNullException(nameof(typeInfo));

            return Pools.Variable.Allocate().Allocate(name, type, typeInfo, isParameter, index);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ssa.VariableFactory" /> class.
        /// </summary>
        /// <param name="type">The type of the variable.</param>
        /// <param name="name">The name of the variable.</param>
        public static VariableFactory VariableFactory(Type type, string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (type == null) throw new ArgumentNullException(nameof(type));

            return Pools.VariableFactory.Allocate().Allocate(type, name);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="Ssa.ConstantInstruction{T}" /> class.
        /// </summary>
        /// <param name="output">The output variable.</param>
        /// <param name="value">The value of the constant.</param>
        /// <returns>The <see cref="ConstantInstruction{T}"/>.</returns>.
        /// <exception cref="System.ArgumentNullException">output</exception>
        public static ConstantInstruction ConstantObjectInstruction(Variable output, object value)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (ReferenceEquals(value, null)) return ConstantInstruction(output, value);
            var inst = AllocateUnknownType<ConstantInstruction>(value.GetType().TypeHandle);
            return inst.AllocateObject(output, value);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="Ssa.ConstantInstruction{T}" /> class.
        /// </summary>
        /// <param name="output">The output variable.</param>
        /// <param name="value">The value of the constant.</param>
        /// <returns>The <see cref="ConstantInstruction{T}"/>.</returns>.
        /// <exception cref="System.ArgumentNullException">output</exception>
        public static ConstantInstruction<T> ConstantInstruction<T>(Variable output, T value)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            return Pools<T>.ConstantInstruction.Allocate().Allocate(output, value);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="Ssa.ReturnInstruction" /> class.
        /// </summary>
        /// <param name="returnValue">The return value.</param>
        /// <returns>
        /// The <see cref="Ssa.ReturnInstruction" />.
        /// </returns>
        public static ReturnInstruction ReturnInstruction() => ReturnInstruction(null);

        /// <summary>
        /// Allocates a new instance of the <see cref="Ssa.ReturnInstruction" /> class.
        /// </summary>
        /// <param name="returnValue">The return value.</param>
        /// <returns>
        /// The <see cref="Ssa.ReturnInstruction" />.
        /// </returns>
        public static ReturnInstruction ReturnInstruction(Variable returnValue)
        {
            return Pools.ReturnInstruction.Allocate().Allocate(returnValue);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="BranchCompareInstruction" /> class
        /// that will always branch to the specified destination.
        /// </summary>
        /// <param name="destination">The destination that will be jumped to.</param>
        /// <returns>The <see cref="BranchCompareInstruction"/>.</returns>
        /// <exception cref="ArgumentNullException">destination</exception>
        public static BranchCompareInstruction BranchCompareInstruction(Block destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            return Pools.BranchCompareInstruction.Allocate().Allocate(destination, null, Comparison.Always, null, null);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="BranchCompareInstruction" /> class
        /// that will check the value of a boolean to determine if a branch occurs.
        /// </summary>
        /// <param name="destination">The destination that will be jumped to.</param>
        /// <param name="value">The single value to compare.</param>
        /// <param name="comparison">The single-valued comparison that will be made.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="comparison" /> is not one of the single-valued comparisons.</exception>
        public static BranchCompareInstruction BranchCompareInstruction(Block destination, Variable value, Comparison comparison)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            switch (comparison)
            {
                case Comparison.True:
                case Comparison.False:
                    break;
                default:
                    break; throw new ArgumentOutOfRangeException(nameof(comparison));
            }
            return Pools.BranchCompareInstruction.Allocate().Allocate(destination, value, comparison, null, null);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="BranchCompareInstruction" /> class
        /// that will perform a comparison in order to determine if a branch occurs.
        /// </summary>
        /// <param name="destination">The destination that will be jumped to.</param>
        /// <param name="left">The <see cref="Variable" /> that participates in the comparison on the left-hand side.</param>
        /// <param name="comparison">The dual-valued comparison that will be made.</param>
        /// <param name="right">The <see cref="Variable" /> that participates in the comparison on the right-hand side.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="comparison" /> is not one of the dual-valued comparisons.</exception>
        public static BranchCompareInstruction BranchCompareInstruction(Block destination, Variable left, Comparison comparison, Variable right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));
            switch (comparison)
            {
                case Comparison.Equal:
                case Comparison.NotEqual:
                case Comparison.GreaterThan:
                case Comparison.GreaterThanOrEqual:
                case Comparison.LessThan:
                case Comparison.LessThanOrEqual:
                    break;
                default:
                    break; throw new ArgumentOutOfRangeException(nameof(comparison));
            }
            return Pools.BranchCompareInstruction.Allocate().Allocate(destination, left, comparison, right, null);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="BranchCompareInstruction" /> class
        /// that will perform a comparison in order to determine if a branch occurs.
        /// </summary>
        /// <param name="destination">The destination that will be jumped to.</param>
        /// <param name="left">The <see cref="Variable" /> that participates in the comparison on the left-hand side.</param>
        /// <param name="comparison">The dual-valued comparison that will be made.</param>
        /// <param name="right">The <see cref="Variable" /> that participates in the comparison on the right-hand side.</param>
        /// <param name="ordered">If set to <c>true</c>, the comparison will be ordered.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// left
        /// or
        /// right
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="comparison" /> is not one of the dual-valued comparisons.</exception>
        public static BranchCompareInstruction BranchCompareInstruction(Block destination, Variable left, Comparison comparison, Variable right, bool ordered)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));
            switch (comparison)
            {
                case Comparison.Equal:
                case Comparison.NotEqual:
                case Comparison.GreaterThan:
                case Comparison.GreaterThanOrEqual:
                case Comparison.LessThan:
                case Comparison.LessThanOrEqual:
                    break;
                default:
                    break; throw new ArgumentOutOfRangeException(nameof(comparison));
            }
            return Pools.BranchCompareInstruction.Allocate().Allocate(destination, left, comparison, right, ordered);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperatorInstruction" /> class.
        /// </summary>
        /// <param name="output">The root variable from which the output variable will be created.</param>
        /// <param name="left">The left variable that participated in the mathematical operation.</param>
        /// <param name="binaryOperator">The binary operator.</param>
        /// <param name="right">The right variable that participated in the mathematical operation.</param>
        public static BinaryOperatorInstruction BinaryOperatorInstruction(Variable output, Variable left, BinaryOperator binaryOperator, Variable right)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));
            return Pools.BinaryOperatorInstruction.Allocate().Allocate(output, left, binaryOperator, right, true, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperatorInstruction" /> class.
        /// </summary>
        /// <param name="output">The root variable from which the output variable will be created.</param>
        /// <param name="left">The left variable that participated in the mathematical operation.</param>
        /// <param name="binaryOperator">The binary operator.</param>
        /// <param name="right">The right variable that participated in the mathematical operation.</param>
        /// <param name="isChecked">A value indicating whether to perform a checked operation.</param>
        public static BinaryOperatorInstruction BinaryOperatorInstruction(Variable output, Variable left, BinaryOperator binaryOperator, Variable right, bool isChecked)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));
            return Pools.BinaryOperatorInstruction.Allocate().Allocate(output, left, binaryOperator, right, isChecked, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperatorInstruction" /> class.
        /// </summary>
        /// <param name="output">The root variable from which the output variable will be created.</param>
        /// <param name="left">The left variable that participated in the mathematical operation.</param>
        /// <param name="binaryOperator">The binary operator.</param>
        /// <param name="right">The right variable that participated in the mathematical operation.</param>
        /// <param name="isChecked">A value indicating whether to perform a checked operation.</param>
        /// <param name="isOrdered">A value indicating whether to perform an ordered operation.</param>
        public static BinaryOperatorInstruction BinaryOperatorInstruction(Variable output, Variable left, BinaryOperator binaryOperator, Variable right, bool isChecked, bool isOrdered)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));
            return Pools.BinaryOperatorInstruction.Allocate().Allocate(output, left, binaryOperator, right, isChecked, isOrdered);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhiInstruction" /> class.
        /// </summary>
        /// <param name="output">The root variable.</param>
        /// <param name="inputs">The input variables to choose from.</param>
        public static PhiInstruction PhiInstruction(Variable output, params Variable[] inputs)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));
            return Pools.PhiInstruction.Allocate().Allocate(output, inputs.ToImmutableArray());
        }
        #endregion

        #region Compilation
        /// <summary>
        /// Allocates a new instance of the <see cref="Services.RootVariableService" /> class.
        /// </summary>
        /// <param name="body">The method body.</param>
        /// <returns>The <see cref="Services.RootVariableService"/>.</returns>
        public static Services.RootVariableService RootVariableService(Body body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            return Pools.RootVariableService.Allocate().Allocate(body);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="Ssa.DynamicBodyTarget" /> class.
        /// </summary>
        /// <param name="body">The method body.</param>
        /// <returns>The <see cref="T:DynamicBodyTarget"/>.</returns>
        public static DynamicBodyTarget DynamicBodyTarget(Body body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            return Pools.DynamicBodyTarget.Allocate().Allocate(body);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="Ssa.DefaultILGenerator" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="il">The <see cref="ILGenerator" />.</param>
        /// <returns>The <see cref="T:DefaultILGenerator"/>.</returns>
        public static DefaultILGenerator DefaultILGenerator(IBodyTarget target, ILGenerator il)
        {
            if (il == null) throw new ArgumentNullException(nameof(il));
            if (target == null) throw new ArgumentNullException(nameof(target));
            return Pools.DefaultILGenerator.Allocate().Allocate(target, il);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="Ssa.Block" /> class.
        /// </summary>
        /// <param name="body">The containing body.</param>
        /// <returns>
        /// The <see cref="Block" />.
        /// </returns>
        internal static Block Block(Body body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            return Pools.Block.Allocate().Allocate(body);
        }

        /// <summary>
        /// Allocates a new instance of the <see cref="Ssa.Body" /> class.
        /// </summary>
        /// <param name="body">The containing body.</param>
        /// <returns>
        /// The <see cref="Body" />.
        /// </returns>
        public static Body Body()
        {
            return Pools.Body.Allocate().Allocate();
        }
        #endregion
    }
}
