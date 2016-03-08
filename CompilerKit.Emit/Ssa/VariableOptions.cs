using System;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents different options for variables.
    /// </summary>
    [Flags]
    public enum VariableOptions
    {
        /// <summary>
        /// The variable behaves normally.
        /// </summary>
        None = 0,
        /// <summary>
        /// The variable is a candidate for only existing on the stack.
        /// </summary>
        StackCandidate = 1,
        /// <summary>
        /// The variable cannot be used on the stack.
        /// </summary>
        StackProhibited = 2,
        /// <summary>
        /// The stack operations
        /// </summary>
        StackOperations = StackCandidate | StackProhibited
    }
}
