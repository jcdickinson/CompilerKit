using System;
using System.Collections.Generic;

namespace CompilerKit.Emit.Ssa.Services
{
    /// <summary>
    /// Represents a <see cref="IVariableService"/> that simply reflects variables
    /// found on the body.
    /// </summary>
    public sealed class RootVariableService : IVariableService, IPooledObject, IDisposable
    {
        /// <summary>
        /// Gets the list of parameter variables.
        /// </summary>
        public IEnumerable<Variable> Parameters => _body.Parameters;

        /// <summary>
        /// Gets the list of local variables.
        /// </summary>
        public IEnumerable<Variable> Locals => _body.Locals;

        private Body _body;

        internal RootVariableService() { }

        internal RootVariableService Allocate(Body body)
        {
            _body = body;
            return this;
        }

        /// <summary>
        /// Gets the variable to use when referring to the
        /// specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>The variable to use instead of <paramref name="variable"/>.</returns>
        public Variable FindVariable(Variable variable) => variable;

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>A value indicating whether the instance was returned to the pool.</returns>
        public bool Free()
        {
            return SsaFactory.Pools.RootVariableService.Free(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _body = null;
        }
    }
}
