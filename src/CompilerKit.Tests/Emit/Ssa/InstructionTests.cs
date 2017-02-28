using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace CompilerKit.Emit.Ssa
{
    public class InstructionTests
    {
        #region Helpers
        private static MethodInfo CompileSimple(Type returnType, Body body)
        {
            var param = body.Parameters.Cast<Variable>().Select(x => x.Type).ToArray();

            var dm = new DynamicMethod("TestMethod", returnType, param);
            var il = SsaFactory.DefaultILGenerator(SsaFactory.DynamicBodyTarget(body), dm.GetILGenerator());

            Optimizers.VariableDestructionOptimizer.Instance.Optimize(il.Target);

            var variables = il.Target.GetService<Services.IVariableService>();

            foreach (var variable in variables.Locals)
                il.Declare(variable);

            foreach (var block in body)
            {
                il.Declare(block);
            }

            foreach (var block in body)
            {
                il.Emit(block);
                foreach (var instruction in block)
                    instruction.CompileTo(il);
            }

            il.Target.Free();
            body.Free();
            il.Free();
            GC.Collect(2);

            return dm;
        }
        #endregion

        #region ConstantInstruction
        [Theory(DisplayName = "ConstantInstruction should perform the correct operation")]
        [MemberData(nameof(GenerateConstants))]
        public void ConstantInstruction_Constant_Compile(Type returnType, object expected)
        {
            var body = SsaFactory.Body();
            var result = body.Locals.Add(returnType, "result");

            var cnst = SsaFactory.ConstantObjectInstruction(result, expected);
            var ret = SsaFactory.ReturnInstruction(cnst.Output);

            body.MainBlock.Add(cnst);
            body.MainBlock.Add(ret);

            var finalMethod = CompileSimple(returnType, body);

            var actual = finalMethod.Invoke(null, new object[0]);

            if (returnType == typeof(string))
                Assert.Equal(expected, actual);
            else
            {
                Expression.Lambda<Action>(Expression.Call(
                    typeof(Assert), "Equal",
                    new[] { returnType },
                    Expression.Constant(expected), Expression.Constant(actual))
                ).Compile(true)();
            }
        }

        private static TheoryData<Type, object> GenerateConstants()
        {
            var result = new TheoryData<Type, object>();

            AddConstants(result, new[] { true, false });
            AddConstants(result, new[] { "Test", null });
            AddConstants(result, Enumerable.Range(0, 200).Select(x => (char)x));
            AddConstants(result, Enumerable.Range(-128, 127).Select(x => (sbyte)x));
            AddConstants(result, Enumerable.Range(-200, 200).Select(x => (short)x));
            AddConstants(result, Enumerable.Range(0, 200).Select(x => (ushort)x));
            AddConstants(result, Enumerable.Range(-200, 200).Select(x => x));
            AddConstants(result, Enumerable.Range(0, 200).Select(x => (uint)x));
            AddConstants(result, Enumerable.Range(-200, 200).Select(x => (long)x));
            AddConstants(result, Enumerable.Range(0, 200).Select(x => (ulong)x));
            AddConstants(result, Enumerable.Range(-200, 200).Select(x => x / 1.2f));
            AddConstants(result, Enumerable.Range(0, 200).Select(x => (ulong)x));

            return result;
        }

        private static void AddConstants<T>(TheoryData<Type, object> data, IEnumerable<T> values)
        {
            var t = typeof(T);
            foreach (var value in values)
                data.Add(t, value);
        }
        #endregion

        #region BranchCompareInstruction
        [Fact(DisplayName = "BranchCompareInstruction Always should always branch")]
        public void BranchCompareInstruction_Always_Compile()
        {
            var body = SsaFactory.Body();
            var target = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var incorrectConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var correctConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(target));
                body.MainBlock.Add(incorrectConstant);
                body.MainBlock.Add(SsaFactory.ReturnInstruction(incorrectConstant.Output));

                target.Add(correctConstant);
                target.Add(SsaFactory.ReturnInstruction(correctConstant.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[0]);
                Assert.Equal("Block 1", actual);
            }
        }

        [Fact(DisplayName = "BranchCompareInstruction True should branch when the value is true")]
        public void BranchCompareInstruction_True_Compile()
        {
            var body = SsaFactory.Body();
            var target = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var param = body.Parameters.Add(typeof(bool), "branch");

                var incorrectConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var correctConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(target, param, Comparison.True));
                body.MainBlock.Add(incorrectConstant);
                body.MainBlock.Add(SsaFactory.ReturnInstruction(incorrectConstant.Output));

                target.Add(correctConstant);
                target.Add(SsaFactory.ReturnInstruction(correctConstant.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[] { false });
                Assert.Equal("Block 0", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { true });
                Assert.Equal("Block 1", actual);
            }
        }

        [Fact(DisplayName = "BranchCompareInstruction Equal should branch when the value matches")]
        public void BranchCompareInstruction_False_Compile()
        {
            var body = SsaFactory.Body();
            var target = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var zero = body.Locals.Add(typeof(int), "zero");
                var param = body.Parameters.Add(typeof(int), "param");

                var testConstant = SsaFactory.ConstantInstruction(zero, 0);
                var incorrectConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var correctConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(testConstant);
                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(target, param, Comparison.Equal, testConstant.Output));
                body.MainBlock.Add(incorrectConstant);
                body.MainBlock.Add(SsaFactory.ReturnInstruction(incorrectConstant.Output));

                target.Add(correctConstant);
                target.Add(SsaFactory.ReturnInstruction(correctConstant.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[] { 0 });
                Assert.Equal("Block 1", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 100 });
                Assert.Equal("Block 0", actual);
            }
        }

        [Theory(DisplayName = "BranchCompareInstruction Equal should branch when the value does not match")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_Equal_Compile(bool signed)
        {
            var body = SsaFactory.Body();
            var target = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var zero = body.Locals.Add(typeof(int), "zero");
                var param = body.Parameters.Add(typeof(int), "param");

                var testConstant = SsaFactory.ConstantInstruction(zero, 0);
                var incorrectConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var correctConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(testConstant);
                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(target, param, Comparison.Equal, testConstant.Output, signed));
                body.MainBlock.Add(incorrectConstant);
                body.MainBlock.Add(SsaFactory.ReturnInstruction(incorrectConstant.Output));

                target.Add(correctConstant);
                target.Add(SsaFactory.ReturnInstruction(correctConstant.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[] { 0 });
                Assert.Equal("Block 1", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 100 });
                Assert.Equal("Block 0", actual);
            }
        }

        [Theory(DisplayName = "BranchCompareInstruction NotEqual should branch when the value does not match")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_NotEqual_Compile(bool signed)
        {
            var body = SsaFactory.Body();
            var target = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var zero = body.Locals.Add(typeof(int), "zero");
                var param = body.Parameters.Add(typeof(int), "param");

                var testConstant = SsaFactory.ConstantInstruction(zero, 0);
                var incorrectConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var correctConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(testConstant);
                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(target, param, Comparison.NotEqual, testConstant.Output, signed));
                body.MainBlock.Add(incorrectConstant);
                body.MainBlock.Add(SsaFactory.ReturnInstruction(incorrectConstant.Output));

                target.Add(correctConstant);
                target.Add(SsaFactory.ReturnInstruction(correctConstant.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[] { 0 });
                Assert.Equal("Block 0", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 100 });
                Assert.Equal("Block 1", actual);
            }
        }

        [Theory(DisplayName = "BranchCompareInstruction GreaterThan should branch when the value is greater")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_GreaterThan_Compile(bool signed)
        {
            var body = SsaFactory.Body();
            var target = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var onehundred = body.Locals.Add(typeof(int), "onehundred");
                var param = body.Parameters.Add(typeof(int), "param");

                var testConstant = SsaFactory.ConstantInstruction(onehundred, 100);
                var incorrectConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var correctConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(testConstant);
                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(target, param, Comparison.GreaterThan, testConstant.Output, signed));
                body.MainBlock.Add(incorrectConstant);
                body.MainBlock.Add(SsaFactory.ReturnInstruction(incorrectConstant.Output));

                target.Add(correctConstant);
                target.Add(SsaFactory.ReturnInstruction(correctConstant.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[] { 99 });
                Assert.Equal("Block 0", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 100 });
                Assert.Equal("Block 0", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 101 });
                Assert.Equal("Block 1", actual);
            }
        }

        [Theory(DisplayName = "BranchCompareInstruction GreaterThanOrEqual should branch when the value is greater or equal")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_GreaterThanOrEqual_Compile(bool signed)
        {
            var body = SsaFactory.Body();
            var target = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var onehundred = body.Locals.Add(typeof(int), "onehundred");
                var param = body.Parameters.Add(typeof(int), "param");

                var testConstant = SsaFactory.ConstantInstruction(onehundred, 100);
                var incorrectConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var correctConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(testConstant);
                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(target, param, Comparison.GreaterThanOrEqual, testConstant.Output, signed));
                body.MainBlock.Add(incorrectConstant);
                body.MainBlock.Add(SsaFactory.ReturnInstruction(incorrectConstant.Output));

                target.Add(correctConstant);
                target.Add(SsaFactory.ReturnInstruction(correctConstant.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[] { 99 });
                Assert.Equal("Block 0", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 100 });
                Assert.Equal("Block 1", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 101 });
                Assert.Equal("Block 1", actual);
            }
        }

        [Theory(DisplayName = "BranchCompareInstruction LessThan should branch when the value is less or equal")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_LessThan_Compile(bool signed)
        {
            var body = SsaFactory.Body();
            var target = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var onehundred = body.Locals.Add(typeof(int), "onehundred");
                var param = body.Parameters.Add(typeof(int), "param");

                var testConstant = SsaFactory.ConstantInstruction(onehundred, 100);
                var incorrectConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var correctConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(testConstant);
                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(target, param, Comparison.LessThan, testConstant.Output, signed));
                body.MainBlock.Add(incorrectConstant);
                body.MainBlock.Add(SsaFactory.ReturnInstruction(incorrectConstant.Output));

                target.Add(correctConstant);
                target.Add(SsaFactory.ReturnInstruction(correctConstant.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[] { 99 });
                Assert.Equal("Block 1", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 100 });
                Assert.Equal("Block 0", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 101 });
                Assert.Equal("Block 0", actual);
            }
        }

        [Theory(DisplayName = "BranchCompareInstruction LessThanOrEqual should branch when the value is less or equal")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_LessThanOrEqual_Compile(bool signed)
        {
            var body = SsaFactory.Body();
            var target = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var onehundred = body.Locals.Add(typeof(int), "onehundred");
                var param = body.Parameters.Add(typeof(int), "param");

                var testConstant = SsaFactory.ConstantInstruction(onehundred, 100);
                var incorrectConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var correctConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(testConstant);
                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(target, param, Comparison.LessThanOrEqual, testConstant.Output, signed));
                body.MainBlock.Add(incorrectConstant);
                body.MainBlock.Add(SsaFactory.ReturnInstruction(incorrectConstant.Output));

                target.Add(correctConstant);
                target.Add(SsaFactory.ReturnInstruction(correctConstant.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[] { 99 });
                Assert.Equal("Block 1", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 100 });
                Assert.Equal("Block 1", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { 101 });
                Assert.Equal("Block 0", actual);
            }
        }
        #endregion

        #region BinaryOperatorInstruction
        [Theory(DisplayName = "BinaryOperatorInstruction should perform the correct operation")]
        [InlineData(typeof(short), typeof(short), BinaryOperator.Add)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.Add)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.Add)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.Add)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.Add)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.Add)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.Add)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.Add)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.Subtract)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.Subtract)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.Subtract)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.Subtract)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.Subtract)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.Subtract)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.Subtract)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.Subtract)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.Multiply)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.Multiply)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.Multiply)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.Multiply)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.Multiply)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.Multiply)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.Multiply)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.Multiply)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.Divide)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.Divide)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.Divide)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.Divide)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.Divide)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.Divide)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.Divide)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.Divide)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.Modulo)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.Modulo)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.Modulo)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.Modulo)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.Modulo)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.Modulo)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.Modulo)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.Modulo)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.Equal)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.Equal)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.Equal)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.Equal)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.Equal)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.Equal)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.Equal)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.Equal)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.NotEqual)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.NotEqual)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.NotEqual)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.NotEqual)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.NotEqual)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.NotEqual)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.NotEqual)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.NotEqual)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.LessThan)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.LessThan)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.LessThan)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.LessThan)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.LessThan)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.LessThan)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.LessThan)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.LessThan)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.LessThanOrEqual)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.LessThanOrEqual)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.LessThanOrEqual)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.LessThanOrEqual)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.LessThanOrEqual)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.LessThanOrEqual)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.LessThanOrEqual)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.LessThanOrEqual)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.GreaterThan)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.GreaterThan)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.GreaterThan)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.GreaterThan)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.GreaterThan)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.GreaterThan)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.GreaterThan)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.GreaterThan)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.GreaterThanOrEqual)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.GreaterThanOrEqual)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.GreaterThanOrEqual)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.GreaterThanOrEqual)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.GreaterThanOrEqual)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.GreaterThanOrEqual)]
        [InlineData(typeof(float), typeof(float), BinaryOperator.GreaterThanOrEqual)]
        [InlineData(typeof(double), typeof(double), BinaryOperator.GreaterThanOrEqual)]

        [InlineData(typeof(short), typeof(int), BinaryOperator.LeftShift)]
        [InlineData(typeof(ushort), typeof(int), BinaryOperator.LeftShift)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.LeftShift)]
        [InlineData(typeof(uint), typeof(int), BinaryOperator.LeftShift)]
        [InlineData(typeof(long), typeof(int), BinaryOperator.LeftShift)]
        [InlineData(typeof(ulong), typeof(int), BinaryOperator.LeftShift)]

        [InlineData(typeof(short), typeof(int), BinaryOperator.RightShift)]
        [InlineData(typeof(ushort), typeof(int), BinaryOperator.RightShift)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.RightShift)]
        [InlineData(typeof(uint), typeof(int), BinaryOperator.RightShift)]
        [InlineData(typeof(long), typeof(int), BinaryOperator.RightShift)]
        [InlineData(typeof(ulong), typeof(int), BinaryOperator.RightShift)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.And)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.And)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.And)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.And)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.And)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.And)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.Or)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.Or)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.Or)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.Or)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.Or)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.Or)]

        [InlineData(typeof(short), typeof(short), BinaryOperator.ExclusiveOr)]
        [InlineData(typeof(ushort), typeof(ushort), BinaryOperator.ExclusiveOr)]
        [InlineData(typeof(int), typeof(int), BinaryOperator.ExclusiveOr)]
        [InlineData(typeof(uint), typeof(uint), BinaryOperator.ExclusiveOr)]
        [InlineData(typeof(long), typeof(long), BinaryOperator.ExclusiveOr)]
        [InlineData(typeof(ulong), typeof(ulong), BinaryOperator.ExclusiveOr)]
        public void Binary_Operator_Compile(Type xT, Type yT, BinaryOperator op)
        {
            var expressionParam1 = Expression.Parameter(xT, "x");
            var expressionParam2 = Expression.Parameter(yT, "y");
            var sourceOfTruthExpression = Expression.MakeBinary((ExpressionType)op, expressionParam1, expressionParam2);
            var returnType = sourceOfTruthExpression.Type;
            var sourceOfTruth = Expression.Lambda(sourceOfTruthExpression, expressionParam1, expressionParam2)
                .Compile();

            var body = SsaFactory.Body();
            var x = body.Parameters.Add(xT, "x");
            var y = body.Parameters.Add(yT, "y");
            var result = body.Locals.Add(returnType, "result");

            var add = SsaFactory.BinaryOperatorInstruction(result, x, op, y, false);
            var ret = SsaFactory.ReturnInstruction(add.Output);

            body.MainBlock.Add(add);
            body.MainBlock.Add(ret);

            var finalMethod = CompileSimple(returnType, body);

            var minX = GetValue(xT, "Min");
            var maxX = GetValue(xT, "Max");
            var minY = GetValue(yT, "Min");
            var maxY = GetValue(yT, "Min");

            if (op == BinaryOperator.LeftShift || op == BinaryOperator.RightShift)
            {
                minY = 2;
                maxY = 4;
            }

            var permutations = new[]
            {
                Tuple.Create(minX, minY),
                Tuple.Create(maxX, maxY),
                Tuple.Create(minX, maxY),
                Tuple.Create(maxX, minY),
            };

            foreach (var pair in permutations)
            {
                var truth = DivideByZeroProtect(() => sourceOfTruth.DynamicInvoke(pair.Item1, pair.Item2));
                var actual = DivideByZeroProtect(() => finalMethod.Invoke(null, new[] { pair.Item1, pair.Item2 }));
                Assert.IsType(truth.GetType(), actual);
                Assert.Equal(truth, actual);
            }
        }

        static object _divideByZero = new object();
        private static object DivideByZeroProtect(Func<object> check)
        {
            try
            {
                return check();
            }
            catch (TargetInvocationException ti) when (ti.GetBaseException() is DivideByZeroException)
            {
                return _divideByZero;
            }
            catch (DivideByZeroException)
            {
                return _divideByZero;
            }
        }

        private static object GetValue(Type t, string type)
        {
            return t.GetTypeInfo().GetField(type + "Value", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }
        #endregion

        #region PhiInstruction
        [Fact(DisplayName = "PhiInstruction should select the correct value from 2 values")]
        public void PhiInstruction_Binary_Compile()
        {
            var body = SsaFactory.Body();
            var trueBranch = body.AddBlock();
            var falseBranch = body.AddBlock();
            var returnBranch = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var param = body.Parameters.Add(typeof(bool), "branch");

                var falseConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var trueConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(trueBranch, param, Comparison.True));
                trueBranch.Add(trueConstant);
                trueBranch.Add(SsaFactory.BranchCompareInstruction(returnBranch));

                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(falseBranch));
                falseBranch.Add(falseConstant);
                falseBranch.Add(SsaFactory.BranchCompareInstruction(returnBranch));

                var phi = SsaFactory.PhiInstruction(body.Locals.Add(val), falseConstant.Output, trueConstant.Output);
                returnBranch.Add(phi);
                returnBranch.Add(SsaFactory.ReturnInstruction(phi.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[] { false });
                Assert.Equal("Block 0", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { true });
                Assert.Equal("Block 1", actual);
            }
        }

        [Fact(DisplayName = "PhiInstruction should select the correct value from itself or another value")]
        public void PhiInstruction_BinarySelf_Compile()
        {
            var body = SsaFactory.Body();
            var trueBranch = body.AddBlock();
            var returnBranch = body.AddBlock();

            using (var val = SsaFactory.VariableFactory(typeof(string), "value"))
            {
                var param = body.Parameters.Add(typeof(bool), "branch");

                var falseConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 0");
                var trueConstant = SsaFactory.ConstantInstruction(body.Locals.Add(val), "Block 1");

                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(trueBranch, param, Comparison.True));
                trueBranch.Add(trueConstant);
                trueBranch.Add(SsaFactory.BranchCompareInstruction(returnBranch));

                body.MainBlock.Add(falseConstant);
                body.MainBlock.Add(SsaFactory.BranchCompareInstruction(returnBranch));

                var phi = SsaFactory.PhiInstruction(body.Locals.Add(val), falseConstant.Output, trueConstant.Output);
                returnBranch.Add(phi);
                returnBranch.Add(SsaFactory.ReturnInstruction(phi.Output));

                var finalMethod = CompileSimple(typeof(string), body);

                var actual = (string)finalMethod.Invoke(null, new object[] { false });
                Assert.Equal("Block 0", actual);

                actual = (string)finalMethod.Invoke(null, new object[] { true });
                Assert.Equal("Block 1", actual);
            }
        }
        #endregion
    }
}
