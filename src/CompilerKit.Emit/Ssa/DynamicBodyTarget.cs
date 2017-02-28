using CompilerKit.Emit.Ssa.Services;
using System;
using System.Collections.Generic;

namespace CompilerKit.Emit.Ssa
{
    /// <summary>
    /// Represents a dynamic method target.
    /// </summary>
    public class DynamicBodyTarget : IBodyTarget, IDisposable
    {
        private readonly Dictionary<RuntimeTypeHandle, object> _services = new Dictionary<RuntimeTypeHandle, object>(RuntimeTypeHandleEqualityComparer.Default);

        /// <summary>
        /// Gets the body that is being compiled in the target.
        /// </summary>
        public Body Body { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicBodyTarget"/> class.
        /// </summary>
        protected internal DynamicBodyTarget() { }

        /// <summary>
        /// Initializes an allocated instance.
        /// </summary>
        /// <param name="body">The method body.</param>
        /// <returns>The <see cref="Block"/> instance.</returns>
        protected internal DynamicBodyTarget Allocate(Body body)
        {
            Body = body;
            SetService<IVariableService>(SsaFactory.RootVariableService(body));
            return this;
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        /// <returns>
        /// A value indicating whether this instance was returned to the pool.
        /// </returns>
        public virtual bool Free() => SsaFactory.Pools.DynamicBodyTarget.Free(this);

        /// <summary>
        /// Gets the service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <returns>The service.</returns>
        public T GetService<T>() => (T)_services[typeof(T).TypeHandle];

        /// <summary>
        /// Sets the service of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="service">The implementation of the service.</param>
        public void SetService<T>(T service)
        {
            if (ReferenceEquals(service, null))
                throw new ArgumentNullException(nameof(service));

            if (_services.TryGetValue(typeof(T).TypeHandle, out var oldService) && oldService is IPooledObject pooled)
                pooled.Free();

            _services[typeof(T).TypeHandle] = service;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> if the method is being called from <see cref="Dispose()"/>;
        /// <c>false</c> if it is being called from a finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Body = null;
                foreach (var svc in _services.Values)
                {
                    if (svc is IPooledObject pooled)
                        pooled.Free();
                }
                _services.Clear();
            }
        }
    }
}
