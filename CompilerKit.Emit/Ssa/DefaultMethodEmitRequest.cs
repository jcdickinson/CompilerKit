using System;
using System.Reflection.Emit;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a request to emit a method.
    /// </summary>
    public class DefaultMethodEmitRequest : IMethodEmitRequest
    {
        /// <summary>
        /// Gets the method builder.
        /// </summary>
        /// <value>
        /// The method builder.
        /// </value>
        public MethodBuilder MethodBuilder { get; }

        private DefaultILGenerator _ilGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMethodEmitRequest"/> class.
        /// </summary>
        /// <param name="methodBuilder">The method builder.</param>
        public DefaultMethodEmitRequest(MethodBuilder methodBuilder)
        {
            if (methodBuilder == null) throw new ArgumentNullException(nameof(methodBuilder));
            MethodBuilder = methodBuilder;
        }

        /// <summary>
        /// Creates the <see cref="IILGenerator" /> generator for the current method.
        /// </summary>
        /// <returns>
        /// The <see cref="IILGenerator" /> generator for the current method.
        /// </returns>
        public virtual IILGenerator CreateILGenerator()
        {
            if (_ilGenerator == null)
                _ilGenerator = new DefaultILGenerator(MethodBuilder.GetILGenerator());
            return _ilGenerator;
        }
    }
}
