using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Floydcom.ForEachAsync.Util
{
    [PublicAPI]
    public static class TaskExtensions
    {
        /// <summary>
        /// Asyncronusly waits for the specified task OR cancellation of the specified token
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>System.Threading.Tasks.Task.</returns>
        /// <exception cref="ArgumentNullException">
        /// task - Task cannot be null
        /// </exception>
        [NotNull]
        public static Task WaitAsync([NotNull] this Task task, CancellationToken cancellationToken)
        {
            Requires.NotNull(task, nameof(task), "Task cannot be null");

            if (!cancellationToken.CanBeCanceled)
                return task;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            return DoWaitAsync(task, cancellationToken);

            static async Task DoWaitAsync(Task task, CancellationToken token)
            {
                await new SynchronizationContextRemover();

                using var cancelTaskSource = new CancellationTokenTaskSource<object>(token);

                await await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false);
            }

        }

        /// <summary>
        /// Asyncronusly waits for the specified task OR cancellation of the specified token
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>System.Threading.Tasks.Task.</returns>
        /// <exception cref="ArgumentNullException">
        /// task - Task cannot be null
        /// </exception>
        [NotNull]
        public static Task<T> WaitAsync<T>([NotNull] this Task<T> task, CancellationToken cancellationToken)
        {
            Requires.NotNull(task, nameof(task), "Task cannot be null");

            if (!cancellationToken.CanBeCanceled)
                return task;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<T>(cancellationToken);

            return DoWaitAsync(task, cancellationToken);

            static async Task<T> DoWaitAsync(Task<T> task, CancellationToken token)
            {
                await new SynchronizationContextRemover();

                using var cancelTaskSource = new CancellationTokenTaskSource<T>(token);

                return await await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false);
            }

        }


        internal sealed class CancellationTokenTaskSource<T> : IDisposable
        {
            /// <summary>
            /// The cancellation token registration, if any. This is <c>null</c> if the registration was not necessary.
            /// </summary>
            private readonly IDisposable _registration;

            /// <summary>
            /// Creates a task for the specified cancellation token, registering with the token if necessary.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token to observe.</param>
            public CancellationTokenTaskSource(CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Task = System.Threading.Tasks.Task.FromCanceled<T>(cancellationToken);
                    return;
                }

                var tcs = new TaskCompletionSource<T>();
                // ReSharper disable ArgumentsStyleLiteral
                _registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken), useSynchronizationContext: false);
                // ReSharper restore ArgumentsStyleLiteral
                Task = tcs.Task;
            }

            /// <summary>
            /// Gets the task for the source cancellation token.
            /// </summary>
            public Task<T> Task { get; }

            /// <summary>
            /// Disposes the cancellation token registration, if any. Note that this may cause <see cref="Task"/> to never complete.
            /// </summary>
            public void Dispose() => _registration?.Dispose();

        }
    }
}
