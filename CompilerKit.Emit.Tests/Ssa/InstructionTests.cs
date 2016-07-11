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
        #region Tools
        private static MethodInfo CompileStaticMethod(Type returnType, Body body)
        {
            var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("CompilerKit.TestOutput"), AssemblyBuilderAccess.RunAndSave);
            var mb = ab.DefineDynamicModule("CompilerKit.TestOutput", "CompilerKit.TestOutput.dll");
            var tb = mb.DefineType("CompilerKit.Test.Class", TypeAttributes.Public | TypeAttributes.BeforeFieldInit | TypeAttributes.Serializable | TypeAttributes.Sealed, typeof(object));
            var tm = tb.DefineMethod("TestMethod", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
                returnType, body.Parameters.Values.Select(x => x.Type).ToArray());

            var request = new DefaultMethodEmitRequest(tm);
            body.CompileTo(request);

            var finalType = tb.CreateType();
            var finalMethod = finalType.GetMethod(tm.Name, BindingFlags.Public | BindingFlags.Static);

            //ab.Save("CompilerKit.TestOutput.dll");
            return finalMethod;
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

            var body = new Body();
            var x = body.Parameters.Add(xT, "x");
            var y = body.Parameters.Add(yT, "y");
            var result = body.Variables.Add(returnType, "result");

            var add = new BinaryOperatorInstruction(result.NextVariable(), x.NextVariable(), op, y.NextVariable())
            {
                Checked = false
            };
            var ret = new ReturnInstruction(add.Output);

            body.MainBlock.Add(add);
            body.MainBlock.Add(ret);

            var finalMethod = CompileStaticMethod(returnType, body);

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
            return t.GetField(type + "Value", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }
        #endregion

        #region ConstantInstruction
        [Theory(DisplayName = "ConstantInstruction should perform the correct operation")]
        [MemberData("GenerateConstants")]
        public void ConstantInstruction_Constant_Compile(string description, object[] constants)
        {
            var returnType = constants[0].GetType();

            foreach (var expected in constants)
            {
                var body = new Body();
                var result = body.Variables.Add(returnType, "result");

                var cnst = new ConstantInstruction(result.NextVariable(), expected);
                var ret = new ReturnInstruction(cnst.Output);

                body.MainBlock.Add(cnst);
                body.MainBlock.Add(ret);

                var finalMethod = CompileStaticMethod(returnType, body);

                var actual = finalMethod.Invoke(null, new object[0]);

                if (returnType == typeof(string))
                    Assert.Equal(expected, actual);
                else
                {
                    Expression.Lambda<Action>(Expression.Call(
                        typeof(Assert), "Equal",
                        new[] { returnType },
                        Expression.Constant(expected), Expression.Constant(actual))
                    ).Compile()();
                }
            }
        }

        public static IEnumerable<object[]> GenerateConstants()
        {
            yield return new object[] { "Boolean", new object[] { true, false } };
            yield return new object[] { "Char", Enumerable.Range(0, 200).Select(x => (char)x).Cast<object>().ToArray() };
            yield return new object[] { "SByte", Enumerable.Range(-128, 127).Select(x => (sbyte)x).Cast<object>().ToArray() };
            yield return new object[] { "Int16", Enumerable.Range(-200, 200).Select(x => (short)x).Cast<object>().ToArray() };
            yield return new object[] { "UInt16", Enumerable.Range(0, 200).Select(x => (ushort)x).Cast<object>().ToArray() };
            yield return new object[] { "Int32", Enumerable.Range(-200, 200).Select(x => x).Cast<object>().ToArray() };
            yield return new object[] { "UInt32", Enumerable.Range(0, 200).Select(x => (uint)x).Cast<object>().ToArray() };
            yield return new object[] { "Int64", Enumerable.Range(-200, 200).Select(x => (long)x).Cast<object>().ToArray() };
            yield return new object[] { "UInt64", Enumerable.Range(0, 200).Select(x => (ulong)x).Cast<object>().ToArray() };
            yield return new object[] { "Single", Enumerable.Range(-200, 200).Select(x => x / 1.2f).Cast<object>().ToArray() };
            yield return new object[] { "Double", Enumerable.Range(0, 200).Select(x => (ulong)x).Cast<object>().ToArray() };
            yield return new object[] { "String", new object[] { "Test", null } };
        }
        #endregion

        #region BranchCompareInstruction
        [Fact(DisplayName = "BranchCompareInstruction Always should always branch")]
        public void BranchCompareInstruction_Always_Compile()
        {
            var body = new Body();
            var target = body.CreateBlock();
            var val = body.Variables.Add(typeof(string), "value");

            var incorrectConstant = new ConstantInstruction(val.NextVariable(), "Block 0");
            var correctConstant = new ConstantInstruction(val.NextVariable(), "Block 1");

            body.MainBlock.Add(new BranchCompareInstruction(target));
            body.MainBlock.Add(incorrectConstant);
            body.MainBlock.Add(new ReturnInstruction(incorrectConstant.Output));

            target.Add(correctConstant);
            target.Add(new ReturnInstruction(correctConstant.Output));

            var finalMethod = CompileStaticMethod(typeof(string), body);

            var actual = (string)finalMethod.Invoke(null, new object[0]);
            Assert.Equal("Block 1", actual);
        }

        [Fact(DisplayName = "BranchCompareInstruction True should branch when the value is true")]
        public void BranchCompareInstruction_True_Compile()
        {
            var body = new Body();
            var target = body.CreateBlock();
            var val = body.Variables.Add(typeof(string), "value");
            var param = body.Parameters.Add(typeof(bool), "branch");

            var incorrectConstant = new ConstantInstruction(val.NextVariable(), "Block 0");
            var correctConstant = new ConstantInstruction(val.NextVariable(), "Block 1");

            body.MainBlock.Add(new BranchCompareInstruction(target, param.NextVariable(), Comparison.True));
            body.MainBlock.Add(incorrectConstant);
            body.MainBlock.Add(new ReturnInstruction(incorrectConstant.Output));

            target.Add(correctConstant);
            target.Add(new ReturnInstruction(correctConstant.Output));

            var finalMethod = CompileStaticMethod(typeof(string), body);

            var actual = (string)finalMethod.Invoke(null, new object[] { false });
            Assert.Equal("Block 0", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { true });
            Assert.Equal("Block 1", actual);
        }

        [Fact(DisplayName = "BranchCompareInstruction Equal should branch when the value matches")]
        public void BranchCompareInstruction_False_Compile()
        {
            var body = new Body();
            var target = body.CreateBlock();
            var val = body.Variables.Add(typeof(string), "value");
            var zero = body.Variables.Add(typeof(int), "zero");
            var param = body.Parameters.Add(typeof(int), "param");

            var testConstant = new ConstantInstruction(zero.NextVariable(), 0);
            var incorrectConstant = new ConstantInstruction(val.NextVariable(), "Block 0");
            var correctConstant = new ConstantInstruction(val.NextVariable(), "Block 1");

            body.MainBlock.Add(testConstant);
            body.MainBlock.Add(new BranchCompareInstruction(target, param.NextVariable(), Comparison.Equal, testConstant.Output));
            body.MainBlock.Add(incorrectConstant);
            body.MainBlock.Add(new ReturnInstruction(incorrectConstant.Output));

            target.Add(correctConstant);
            target.Add(new ReturnInstruction(correctConstant.Output));

            var finalMethod = CompileStaticMethod(typeof(string), body);

            var actual = (string)finalMethod.Invoke(null, new object[] { 0 });
            Assert.Equal("Block 1", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 100 });
            Assert.Equal("Block 0", actual);
        }

        [Theory(DisplayName = "BranchCompareInstruction Equal should branch when the value does not match")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_Equal_Compile(bool signed)
        {
            var body = new Body();
            var target = body.CreateBlock();
            var val = body.Variables.Add(typeof(string), "value");
            var zero = body.Variables.Add(typeof(int), "zero");
            var param = body.Parameters.Add(typeof(int), "param");

            var testConstant = new ConstantInstruction(zero.NextVariable(), 0);
            var incorrectConstant = new ConstantInstruction(val.NextVariable(), "Block 0");
            var correctConstant = new ConstantInstruction(val.NextVariable(), "Block 1");

            body.MainBlock.Add(testConstant);
            body.MainBlock.Add(new BranchCompareInstruction(target, param.NextVariable(), Comparison.Equal, testConstant.Output)
            {
                Ordered = signed
            });
            body.MainBlock.Add(incorrectConstant);
            body.MainBlock.Add(new ReturnInstruction(incorrectConstant.Output));

            target.Add(correctConstant);
            target.Add(new ReturnInstruction(correctConstant.Output));

            var finalMethod = CompileStaticMethod(typeof(string), body);

            var actual = (string)finalMethod.Invoke(null, new object[] { 0 });
            Assert.Equal("Block 1", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 100 });
            Assert.Equal("Block 0", actual);
        }

        [Theory(DisplayName = "BranchCompareInstruction NotEqual should branch when the value does not match")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_NotEqual_Compile(bool signed)
        {
            var body = new Body();
            var target = body.CreateBlock();
            var val = body.Variables.Add(typeof(string), "value");
            var zero = body.Variables.Add(typeof(int), "zero");
            var param = body.Parameters.Add(typeof(int), "param");

            var testConstant = new ConstantInstruction(zero.NextVariable(), 0);
            var incorrectConstant = new ConstantInstruction(val.NextVariable(), "Block 0");
            var correctConstant = new ConstantInstruction(val.NextVariable(), "Block 1");

            body.MainBlock.Add(testConstant);
            body.MainBlock.Add(new BranchCompareInstruction(target, param.NextVariable(), Comparison.NotEqual, testConstant.Output)
            {
                Ordered = signed
            });
            body.MainBlock.Add(incorrectConstant);
            body.MainBlock.Add(new ReturnInstruction(incorrectConstant.Output));

            target.Add(correctConstant);
            target.Add(new ReturnInstruction(correctConstant.Output));

            var finalMethod = CompileStaticMethod(typeof(string), body);

            var actual = (string)finalMethod.Invoke(null, new object[] { 0 });
            Assert.Equal("Block 0", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 100 });
            Assert.Equal("Block 1", actual);
        }

        [Theory(DisplayName = "BranchCompareInstruction GreaterThan should branch when the value is greater")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_GreaterThan_Compile(bool signed)
        {
            var body = new Body();
            var target = body.CreateBlock();
            var val = body.Variables.Add(typeof(string), "value");
            var onehundred = body.Variables.Add(typeof(int), "onehundred");
            var param = body.Parameters.Add(typeof(int), "param");

            var testConstant = new ConstantInstruction(onehundred.NextVariable(), 100);
            var incorrectConstant = new ConstantInstruction(val.NextVariable(), "Block 0");
            var correctConstant = new ConstantInstruction(val.NextVariable(), "Block 1");

            body.MainBlock.Add(testConstant);
            body.MainBlock.Add(new BranchCompareInstruction(target, param.NextVariable(), Comparison.GreaterThan, testConstant.Output)
            {
                Ordered = signed
            });
            body.MainBlock.Add(incorrectConstant);
            body.MainBlock.Add(new ReturnInstruction(incorrectConstant.Output));

            target.Add(correctConstant);
            target.Add(new ReturnInstruction(correctConstant.Output));

            var finalMethod = CompileStaticMethod(typeof(string), body);

            var actual = (string)finalMethod.Invoke(null, new object[] { 99 });
            Assert.Equal("Block 0", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 100 });
            Assert.Equal("Block 0", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 101 });
            Assert.Equal("Block 1", actual);
        }

        [Theory(DisplayName = "BranchCompareInstruction GreaterThanOrEqual should branch when the value is greater or equal")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_GreaterThanOrEqual_Compile(bool signed)
        {
            var body = new Body();
            var target = body.CreateBlock();
            var val = body.Variables.Add(typeof(string), "value");
            var onehundred = body.Variables.Add(typeof(int), "onehundred");
            var param = body.Parameters.Add(typeof(int), "param");

            var testConstant = new ConstantInstruction(onehundred.NextVariable(), 100);
            var incorrectConstant = new ConstantInstruction(val.NextVariable(), "Block 0");
            var correctConstant = new ConstantInstruction(val.NextVariable(), "Block 1");

            body.MainBlock.Add(testConstant);
            body.MainBlock.Add(new BranchCompareInstruction(target, param.NextVariable(), Comparison.GreaterThanOrEqual, testConstant.Output)
            {
                Ordered = signed
            });
            body.MainBlock.Add(incorrectConstant);
            body.MainBlock.Add(new ReturnInstruction(incorrectConstant.Output));

            target.Add(correctConstant);
            target.Add(new ReturnInstruction(correctConstant.Output));

            var finalMethod = CompileStaticMethod(typeof(string), body);

            var actual = (string)finalMethod.Invoke(null, new object[] { 99 });
            Assert.Equal("Block 0", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 100 });
            Assert.Equal("Block 1", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 101 });
            Assert.Equal("Block 1", actual);
        }

        [Theory(DisplayName = "BranchCompareInstruction LessThan should branch when the value is greater or equal")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_LessThan_Compile(bool signed)
        {
            var body = new Body();
            var target = body.CreateBlock();
            var val = body.Variables.Add(typeof(string), "value");
            var onehundred = body.Variables.Add(typeof(int), "onehundred");
            var param = body.Parameters.Add(typeof(int), "param");

            var testConstant = new ConstantInstruction(onehundred.NextVariable(), 100);
            var incorrectConstant = new ConstantInstruction(val.NextVariable(), "Block 0");
            var correctConstant = new ConstantInstruction(val.NextVariable(), "Block 1");

            body.MainBlock.Add(testConstant);
            body.MainBlock.Add(new BranchCompareInstruction(target, param.NextVariable(), Comparison.LessThan, testConstant.Output)
            {
                Ordered = signed
            });
            body.MainBlock.Add(incorrectConstant);
            body.MainBlock.Add(new ReturnInstruction(incorrectConstant.Output));

            target.Add(correctConstant);
            target.Add(new ReturnInstruction(correctConstant.Output));

            var finalMethod = CompileStaticMethod(typeof(string), body);

            var actual = (string)finalMethod.Invoke(null, new object[] { 99 });
            Assert.Equal("Block 1", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 100 });
            Assert.Equal("Block 0", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 101 });
            Assert.Equal("Block 0", actual);
        }

        [Theory(DisplayName = "BranchCompareInstruction LessThanOrEqual should branch when the value is greater or equal")]
        [InlineData(false), InlineData(true)]
        public void BranchCompareInstruction_LessThanOrEqual_Compile(bool signed)
        {
            var body = new Body();
            var target = body.CreateBlock();
            var val = body.Variables.Add(typeof(string), "value");
            var onehundred = body.Variables.Add(typeof(int), "onehundred");
            var param = body.Parameters.Add(typeof(int), "param");

            var testConstant = new ConstantInstruction(onehundred.NextVariable(), 100);
            var incorrectConstant = new ConstantInstruction(val.NextVariable(), "Block 0");
            var correctConstant = new ConstantInstruction(val.NextVariable(), "Block 1");

            body.MainBlock.Add(testConstant);
            body.MainBlock.Add(new BranchCompareInstruction(target, param.NextVariable(), Comparison.LessThanOrEqual, testConstant.Output)
            {
                Ordered = signed
            });
            body.MainBlock.Add(incorrectConstant);
            body.MainBlock.Add(new ReturnInstruction(incorrectConstant.Output));

            target.Add(correctConstant);
            target.Add(new ReturnInstruction(correctConstant.Output));

            var finalMethod = CompileStaticMethod(typeof(string), body);

            var actual = (string)finalMethod.Invoke(null, new object[] { 99 });
            Assert.Equal("Block 1", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 100 });
            Assert.Equal("Block 1", actual);

            actual = (string)finalMethod.Invoke(null, new object[] { 101 });
            Assert.Equal("Block 0", actual);
        }
        #endregion
    }
}
