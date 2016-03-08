using System;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents the different emit options.
    /// </summary>
    [Flags]
    public enum EmitOptions
    {
        /// <summary>
        /// No options are present.
        /// </summary>
        None = 0,
        /// <summary>
        /// The option checks for arithmetic overflow and throws a
        /// <see cref="OverflowException"/> if one occurs.
        /// </summary>
        Checked = 1,
        /// <summary>
        /// The operation considers sign (integral values) or considers order
        /// (real values).
        /// </summary>
        SignedOrOrdered = 2
    }
}
