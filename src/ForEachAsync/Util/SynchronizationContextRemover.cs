using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Floydcom.ForEachAsync.Util
{

    internal struct SynchronizationContextRemover : INotifyCompletion
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerHidden]
        public bool IsCompleted => SynchronizationContext.Current == null;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerHidden, DebuggerStepThrough]
        public override bool Equals(object obj) =>
            obj is SynchronizationContextRemover sync && sync.IsCompleted == IsCompleted;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerHidden, DebuggerStepThrough]
        public override int GetHashCode() => 0;

        [EditorBrowsable(EditorBrowsableState.Never)]
        void INotifyCompletion.OnCompleted(Action continuation)
        {
            var prevContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                continuation();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevContext);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerHidden, DebuggerStepThrough]
        public SynchronizationContextRemover GetAwaiter() => this;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerHidden, DebuggerStepThrough]
        public void GetResult() { }
    }
}
