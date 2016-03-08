using System.Reflection.Emit;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents an emit request.
    /// </summary>
    public interface IMethodEmitRequest
    {
        /// <summary>
        /// Gets the method builder for the method being compiled.
        /// </summary>
        /// <value>
        /// The <see cref="MethodBuilder"/> for the method being compiled.
        /// </value>
        MethodBuilder MethodBuilder { get; }

        /// <summary>
        /// Creates the <see cref="IILGenerator"/> generator for the current method.
        /// </summary>
        /// <returns>The <see cref="IILGenerator"/> generator for the current method.</returns>
        IILGenerator CreateILGenerator();
    }
}
