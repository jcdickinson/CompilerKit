using System;
using System.Collections.Generic;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents methods to access and alter a collection of <see cref="RootVariable"/> objects.
    /// </summary>
    public interface IRootVariableCollection : IEnumerable<RootVariable>, IReadOnlyDictionary<string, RootVariable>
    {
        /// <summary>
        /// Declares a new variable with an automatic name.
        /// </summary>
        /// <param name="type">The type of the variable.</param>
        /// <returns>
        /// The declared variable.
        /// </returns>
        RootVariable Add(Type type);

        /// <summary>
        /// Declares a new variable with the specified name.
        /// </summary>
        /// <param name="type">The type of the variable.</param>
        /// <param name="name">The name of the variable.</param>
        /// <returns>
        /// The declared variable.
        /// </returns>
        RootVariable Add(Type type, string name);

        /// <summary>
        /// Gets the index to the specified variable as it
        /// would appear in the method declaration.
        /// </summary>
        /// <param name="variable">The variable to get the index of.</param>
        /// <returns>The index of the specified variable.</returns>
        int IndexOf(Variable variable);
    }
}
