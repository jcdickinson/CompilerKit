#if DETECT_LEAKS
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#endif

namespace CompilerKit.Runtime
{
    public partial class ObjectPool<T>
    {
#       if DETECT_LEAKS
        private static Lazy<Type> _stackTraceType = new Lazy<Type>(() => Type.GetType("System.Diagnostics.StackTrace"));

        private static object CaptureStackTrace()
        {
            return Activator.CreateInstance(_stackTraceType.Value);
        }

        private static readonly ConditionalWeakTable<T, LeakTracker> _leakTrackers = new ConditionalWeakTable<T, LeakTracker>();

        private class LeakTracker : IDisposable
        {
            private volatile bool _disposed;

#           if TRACE_LEAKS
            public volatile object Trace = null;
#           endif

            public void Dispose()
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }

            private string GetTrace()
            {
#               if TRACE_LEAKS
                return Trace == null ? "" : Trace.ToString();
#               else
                return "Leak tracing information is disabled. Define TRACE_LEAKS on ObjectPool`1.cs to get more info \n";
#               endif
            }

            ~LeakTracker()
            {
                if (!_disposed && !Environment.HasShutdownStarted)
                {
                    var trace = GetTrace();

                    // If you are seeing this message it means that object has been allocated from the pool 
                    // and has not been returned back. This is not critical, but turns pool into rather 
                    // inefficient kind of "new".
                    Debug.WriteLine($"TRACEOBJECTPOOLLEAKS_BEGIN\nPool detected potential leaking of {typeof(T)}. \n Location of the leak: \n {GetTrace()} TRACEOBJECTPOOLLEAKS_END");
                }
            }
        }

        private void AllocateTracker(T instance)
        {
            var tracker = new LeakTracker();
            _leakTrackers.Add(instance, tracker);

#           if TRACE_LEAKS
            var frame = CaptureStackTrace();
            tracker.Trace = frame;
#           endif
        }
        
        /// <summary>
        /// Forgets the specified instance.
        /// </summary>
        /// <param name="old">The instance to forget.</param>
        public void Forget(T old)
        {
            if (_leakTrackers.TryGetValue(old, out var tracker))
            {
                tracker.Dispose();
                _leakTrackers.Remove(old);
            }
            else
            {
                Debug.Assert(false, "freeing untracked object?");
            }
        }

#       else

        private void AllocateTracker(T instance) { }

        /// <summary>
        /// Forgets the specified instance.
        /// </summary>
        /// <param name="old">The instance to forget.</param>
        /// <param name="replacement">The instance to replace the old instance with.</param>
        public void Forget(T old, T replacement = null) { }
        
#       endif
    }
}
