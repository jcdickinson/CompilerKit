using System.Linq.Expressions;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents the different binary operators.
    /// </summary>
    public enum BinaryOperator
    {
        /// <summary>
        /// The binary operator that performs mathematical addition.
        /// </summary>
        Add = ExpressionType.Add,
        /// <summary>
        /// The binary operator that performs mathematical subtraction.
        /// </summary>
        Subtract = ExpressionType.Subtract,

        /// <summary>
        /// The binary operator that performs mathematical multiplication.
        /// </summary>
        Multiply = ExpressionType.Multiply,
        /// <summary>
        /// The binary operator that performs mathematical division.
        /// </summary>
        Divide = ExpressionType.Divide,
        /// <summary>
        /// The binary operator that performs mathematical modulus.
        /// </summary>
        Modulo = ExpressionType.Modulo,

        /// <summary>
        /// The binary operator that determines if two numbers are equal.
        /// </summary>
        Equal = ExpressionType.Equal,
        /// <summary>
        /// The binary operator that determines if two numbers are not equal.
        /// </summary>
        NotEqual = ExpressionType.NotEqual,
        /// <summary>
        /// The binary operator that determines if one number is less than another.
        /// </summary>
        LessThan = ExpressionType.LessThan,
        /// <summary>
        /// The binary operator that determines if one number is less than, or equal to, another.
        /// </summary>
        LessThanOrEqual = ExpressionType.LessThanOrEqual,
        /// <summary>
        /// The binary operator that determines if one number is greater than another.
        /// </summary>
        GreaterThan = ExpressionType.GreaterThan,
        /// <summary>
        /// The binary operator that determines if one number is greater than, or equal to, another.
        /// </summary>
        GreaterThanOrEqual = ExpressionType.GreaterThanOrEqual,

        /// <summary>
        /// The binary operator that performs a binary left-shift.
        /// </summary>
        LeftShift = ExpressionType.LeftShift,
        /// <summary>
        /// The binary operator that performs a binary right-shift.
        /// </summary>
        RightShift = ExpressionType.RightShift,

        /// <summary>
        /// The binary operator that performs a binary AND between two numbers.
        /// </summary>
        And = ExpressionType.And,
        /// <summary>
        /// The binary operator that performs a binary OR between two numbers.
        /// </summary>
        Or = ExpressionType.Or,
        /// <summary>
        /// The binary operator that performs a binary XOR between two numbers.
        /// </summary>
        ExclusiveOr = ExpressionType.ExclusiveOr
    }
}
