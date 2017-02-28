using System.Linq.Expressions;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents the different types of branch comparison options.
    /// </summary>
    public enum Comparison
    {
        /// <summary>
        /// Always branch.
        /// </summary>
        Always = ExpressionType.Goto,

        /// <summary>
        /// Branch if the value is considered to be <c>true</c> or non-<c>null</c>.
        /// </summary>
        True = ExpressionType.IsTrue,
        /// <summary>
        /// Branch if the value is considered to be <c>false</c> or <c>null</c>.
        /// </summary>
        False = ExpressionType.IsFalse,

        /// <summary>
        /// Branch if the two values are equal.
        /// </summary>
        Equal = ExpressionType.Equal,
        /// <summary>
        /// Branch if the two values are not equal.
        /// </summary>
        NotEqual = ExpressionType.NotEqual,
        /// <summary>
        /// Branch if the first value is greater than the second.
        /// </summary>
        GreaterThan = ExpressionType.GreaterThan,
        /// <summary>
        /// Branch if the first value is greater than or equal to the second.
        /// </summary>
        GreaterThanOrEqual = ExpressionType.GreaterThanOrEqual,
        /// <summary>
        /// Branch if the first value is less than the second.
        /// </summary>
        LessThan = ExpressionType.LessThan,
        /// <summary>
        /// Branch if the first value is less than or equal to the second.
        /// </summary>
        LessThanOrEqual = ExpressionType.LessThanOrEqual,
    }
}
