using CompilerKit.Emit.Ssa.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a compilation target.
    /// </summary>
    public interface IBodyTarget : IPooledObject
    {
        /// <summary>
        /// Gets the body that is being compiled in the target.
        /// </summary>
        Body Body { get; }

        /// <summary>
        /// Gets the service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <returns>The service.</returns>
        T GetService<T>();

        /// <summary>
        /// Sets the service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="service">The implementation of the service.</param>
        void SetService<T>(T service);
    }
}
