using System.Collections.Generic;
using System.Collections.Immutable;

namespace CompilerKit.Emit.Ssa.Services
{
    /// <summary>
    /// Represents a service that looks up variables.
    /// </summary>
    public interface IVariableService
    {
        /// <summary>
        /// Gets the list of parameter variables.
        /// </summary>
        IEnumerable<Variable> Parameters { get; }

        /// <summary>
        /// Gets the list of local variables.
        /// </summary>
        IEnumerable<Variable> Locals { get; }
        
        /// <summary>
        /// Gets the variable to use when referring to the
        /// specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>The variable to use instead of <paramref name="variable"/>.</returns>
        Variable FindVariable(Variable variable);
    }
}
