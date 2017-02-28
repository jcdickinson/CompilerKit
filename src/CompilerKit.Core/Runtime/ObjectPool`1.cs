using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CompilerKit.Runtime
{
    /// <summary>
    /// Generic implementation of object pooling pattern with predefined pool size limit.
    /// </summary>
    /// <typeparam name="T">The type of allocated instance.</typeparam>
    /// <remarks>
    /// The main purpose is that limited number of frequently used objects can be kept
    /// in the pool for further recycling.
    /// <list type="number"><item><description>
    /// It is not the goal to keep all returned objects. Pool is not meant for
    /// storage. If there is no space in the pool, extra returned objects will be dropped.
    /// </description></item><item><description>
    /// It is implied that if object was obtained from a pool, the caller will return it back in
    /// a relatively short time. Keeping checked out objects for long durations is ok, but
    /// reduces usefulness of pooling. Just new up your own.
    /// </description></item></list>
    /// Not returning objects to the pool in not detrimental to the pool's work, but is a bad practice.
    /// </remarks>
    [DebuggerDisplay("{_items,nq}")]
    public sealed partial class ObjectPool<T> where T : class
    {
        [DebuggerDisplay("{Value,nq}")]
        private struct Element
        {
            internal T Value;
        }

        /// <summary>
        /// Represents a reference to a method that creates an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        public delegate T Factory();
        
        private T _firstItem;
        private readonly Element[] _items;
        private readonly Factory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
        /// </summary>
        /// <param name="factory">The factory delegate.</param>
        public ObjectPool(Factory factory)
            : this(factory, Environment.ProcessorCount * 2)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
        /// </summary>
        /// <param name="factory">The factory delegate.</param>
        /// <param name="size">The size.</param>
        public ObjectPool(Factory factory, int size)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (size < 2) throw new ArgumentOutOfRangeException(nameof(size));

            _factory = factory;
            _items = new Element[size - 1];
        }

        private T CreateInstance()
        {
            var inst = _factory();
            return inst;
        }

        /// <summary>
        /// Allocates an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The allocated instance.</returns>
        public T Allocate()
        {
            T inst = _firstItem;
            if (inst == null || inst != Interlocked.CompareExchange(ref _firstItem, null, inst))
                inst = AllocateSlow();
            AllocateTracker(inst);
            return inst;
        }

        private T AllocateSlow()
        {
            var items = _items;

            for (int i = 0; i < items.Length; i++)
            {
                T inst = items[i].Value;
                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref items[i].Value, null, inst))
                        return inst;
                }
            }

            return CreateInstance();
        }

        /// <summary>
        /// Frees the specified instance.
        /// </summary>
        /// <param name="obj">The instance to free.</param>
        /// <returns><c>true</c> if the object was returned to the pool; otherwise, <c>false</c>.</returns>
        public bool Free(T obj)
        {
            if (ReferenceEquals(obj, null))
            {
                Debug.Assert(false, "freeing null?");
                return true;
            }

            Validate(obj);
            Forget(obj);

            if (_firstItem == null)
            {
                (obj as IDisposable)?.Dispose();
                _firstItem = obj;
                return true;
            }
            else
            {
                return FreeSlow(obj);
            }
        }

        /// <summary>
        /// Frees the specified instance.
        /// </summary>
        /// <param name="collection">The collection to free.</param>
        public void Free<TCollection>(TCollection collection)
            where TCollection : IEnumerable<T>
        {
            foreach(var inst in collection)
            {
                if (!Free(inst)) return;
            }
        }

        private bool FreeSlow(T obj)
        {
            var items = _items;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Value == null)
                {
                    (obj as IDisposable)?.Dispose();
                    items[i].Value = obj;
                    return true;
                }
            }
            return false;
        }

        [Conditional("DEBUG")]
        private void Validate(object obj)
        {
            Debug.Assert(_firstItem != obj, "freeing twice?");

            var items = _items;
            for (int i = 0; i < items.Length; i++)
            {
                var value = items[i].Value;
                if (value == null)
                {
                    return;
                }

                Debug.Assert(value != obj, "freeing twice?");
            }
        }
    }
}
