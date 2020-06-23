using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Floydcom.ForEachAsync.Util;
using JetBrains.Annotations;

namespace Floydcom.ForEachAsync
{
    /// <summary>
    /// Class EnumerableAsyncExtensions. Extension Methods for operating on a collection asynchronously
    /// </summary>
    [PublicAPI]
    public static class EnumerableAsyncExtensions
    {
        /// <summary>
        /// Unbounded Concurrent Iteration. All Tasks are queued concurrently without batching
        /// </summary>
        public const int UnboundedConcurrent = 0;

        /// <summary>
        /// Serial Iteration. Tasks are executed and awaited in order (similar to a normal loop)
        /// </summary>
        public const int Serial = 1;

        private const TaskCreationOptions DenyAttach = TaskCreationOptions.DenyChildAttach;

        /// <summary>
        /// Concurrently invokes the specified asynchronous action for all elements in the sequence and asynchronously waits for completion
        /// of all queued work. This method optionaly queues work using the specified Task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="body">The <see cref="Task"/> returning action to perform for each element.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// body - Body cannot be null
        /// </exception>
        [NotNull]
        public static Task ForEachAsync<[CanBeNull] T>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            [NotNull] Func<T, Task> body,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(body, nameof(body), "Body cannot be null");

            return ForEachAsyncImpl(sequence, UnboundedConcurrent, (item, _, __) => body(item), token, scheduler);
        }

        /// <summary>
        /// Concurrently invokes the specified asynchronous action for all elements (and their indicies) in the sequence and asynchronously waits for completion
        /// of all queued work. This method optionaly queues work using the specified Task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="body">The <see cref="Task"/> returning action to perform for each element. This delegate receives as its parameters the item and its original index.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// body - Body cannot be null
        /// </exception>
        [NotNull]
        public static Task ForEachAsync<[CanBeNull] T>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            [NotNull] Func<T, long, Task> body,
            CancellationToken token = default, [CanBeNull] TaskScheduler scheduler = null)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(body, nameof(body), "Body cannot be null");

            return ForEachAsyncImpl(sequence, UnboundedConcurrent, (item, idx, _) => body(item, idx), token, scheduler);
        }

        /// <summary>
        /// Concurrently invokes the specified asynchronous action for all elements (and their indicies) in the sequence and asynchronously waits for completion
        /// of all queued work. This method optionaly queues work using the specified Task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="body">The <see cref="Task"/> returning action to perform for each element. This delegate receives as its parameters the item,
        /// its original index and the original <see cref="CancellationToken"/>.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// body - Body cannot be null
        /// </exception>
        [NotNull]
        public static Task ForEachAsync<[CanBeNull] T>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            [NotNull] Func<T, long, CancellationToken, Task> body,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null
        ) => ForEachAsync(sequence, UnboundedConcurrent, body, token, scheduler); // delegate signatures match so pass thru directly (not a composite delegate)

        /// <summary>
        /// Performs the specified asynchronous action for each element in the sequence and asynchronously waits for completion of all queued work.
        /// The actions may be performed serially or concurrently (all at once or limited by <paramref name="degreesOfParallelism"/>),
        /// optionaly using the specified task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="degreesOfParallelism">The number of tasks to process concurrently. When <c>0</c> all tasks will run concurrently. When <c>1</c> actions will be invoked
        /// serially (similar to a normal foreach loop) either A. on the current thread (when <paramref name="scheduler"/> is <c>null</c> or B. queued as a single task
        /// to the specified <paramref name="scheduler"/>. Otherwise, this value indicates the number of groups <paramref name="sequence"/> will be partitioned into, running
        /// no more than this number of tasks concurrently.</param>
        /// <param name="body">The <see cref="Task"/> returning action to perform for each element</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.
        /// When <c>null</c> and <paramref name="degreesOfParallelism"/> is <c>1</c>, work is queued beginning on the current thread.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// body - Body cannot be null
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// degreesOfParallelism - DOP must be greater than or equal to 0
        /// </exception>
        [NotNull]
        public static Task ForEachAsync<[CanBeNull] T>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, Task> body,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(body, nameof(body), "Body cannot be null");
            Requires.Range(degreesOfParallelism >= 0, nameof(degreesOfParallelism), "DOP must be greater than or equal to 0");

            return ForEachAsyncImpl(sequence, degreesOfParallelism, (item, _, __) => body(item), token, scheduler);
        }

        /// <summary>
        /// Performs the specified asynchronous action for each element (and its index) in the sequence and asynchronously waits for completion of all queued work.
        /// The actions may be performed serially or concurrently (all at once or limited by <paramref name="degreesOfParallelism"/>),
        /// optionaly using the specified task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="degreesOfParallelism">The number of tasks to process concurrently. When <c>0</c> all tasks will run concurrently. When <c>1</c> actions will be invoked
        /// serially (similar to a normal foreach loop) either A. on the current thread (when <paramref name="scheduler"/> is <c>null</c> or B. queued as a single task
        /// to the specified <paramref name="scheduler"/>. Otherwise, this value indicates the number of groups <paramref name="sequence"/> will be partitioned into, running
        /// no more than this number of tasks concurrently.</param>
        /// <param name="body">The <see cref="Task"/> returning action to perform for each element. This delegate receives as its parameters the item and its original index.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.
        /// When <c>null</c> and <paramref name="degreesOfParallelism"/> is <c>1</c>, work is queued beginning on the current thread.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// body - Body cannot be null
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// degreesOfParallelism - DOP must be greater than or equal to 0
        /// </exception>
        [NotNull]
        public static Task ForEachAsync<[CanBeNull] T>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, Task> body,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(body, nameof(body), "Body cannot be null");
            Requires.Range(degreesOfParallelism >= 0, nameof(degreesOfParallelism), "DOP must be greater than or equal to 0");

            return ForEachAsyncImpl(sequence, degreesOfParallelism, (item, idx, _) => body(item, idx), token, scheduler);
        }

        /// <summary>
        /// Performs the specified asynchronous action for each element (and its index) in the sequence and asynchronously waits for completion of all queued work.
        /// The actions may be performed serially or concurrently (all at once or limited by <paramref name="degreesOfParallelism"/>),
        /// optionaly using the specified task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="degreesOfParallelism">The number of tasks to process concurrently. When <c>0</c> all tasks will run concurrently. When <c>1</c> actions will be invoked
        /// serially (similar to a normal foreach loop) either A. on the current thread (when <paramref name="scheduler"/> is <c>null</c> or B. queued as a single task
        /// to the specified <paramref name="scheduler"/>. Otherwise, this value indicates the number of groups <paramref name="sequence"/> will be partitioned into, running
        /// no more than this number of tasks concurrently. </param>
        /// <param name="body">The <see cref="Task"/> returning action to perform for each element. This delegate receives as its parameters the item,
        /// its original index and the original <see cref="CancellationToken"/>.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.
        /// When <c>null</c> and <paramref name="degreesOfParallelism"/> is <c>1</c>, work is queued beginning on the current thread.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// body - Body cannot be null
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// degreesOfParallelism - DOP must be greater than or equal to 0
        /// </exception>
        [NotNull]
        public static Task ForEachAsync<[CanBeNull] T>(
            [NotNull, ItemCanBeNull, InstantHandle] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, CancellationToken, Task> body,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {

            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(body, nameof(body), "Body cannot be null");
            Requires.Range(degreesOfParallelism >= 0, nameof(degreesOfParallelism), "DOP must be greater than or equal to 0");

            return ForEachAsyncImpl(sequence, degreesOfParallelism, body, token, scheduler);
        }

        /// <summary>
        /// Concurrently invokes the specified asynchronous selector against all elements in the sequence and asynchronously waits for completion
        /// of all queued work, returning the results. This method optionaly queues work using the specified Task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the source sequence</typeparam>
        /// <typeparam name="TResult">Type of item in the result sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="selector">The <see cref="Task{TResult}"/> returning func to apply to each element.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// selector - Selector cannot be null
        /// </exception>
        [NotNull]
        public static Task<TResult[]> SelectAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            [NotNull] Func<T, Task<TResult>> selector,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(selector, nameof(selector), "Selector cannot be null");

            return SelectAsyncImpl(sequence, UnboundedConcurrent, (item, _, __) => selector(item), token, scheduler);
        }


        [NotNull]
        public static IAsyncEnumerable<TResult> SelectEnumerableAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            [NotNull] Func<T, Task<TResult>> selector,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(selector, nameof(selector), "Selector cannot be null");

            return SelectEnumerableAsyncImpl(sequence, UnboundedConcurrent, (item, _, __) => selector(item), token, scheduler);
        }


        /// <summary>
        /// Concurrently invokes the specified asynchronous selector against all elements (and their indicies) in the sequence and asynchronously waits for completion
        /// of all queued work, returning the results. This method optionaly queues work using the specified Task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the source sequence</typeparam>
        /// <typeparam name="TResult">Type of item in the result sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="selector">The <see cref="Task{TResult}"/> returning func to apply to each element. This delegate receives as its parameters the item and its original index.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// selector - Selector cannot be null
        /// </exception>
        [NotNull]
        public static Task<TResult[]> SelectAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            [NotNull] Func<T, long, Task<TResult>> selector,
            CancellationToken token = default, [CanBeNull] TaskScheduler scheduler = null)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(selector, nameof(selector), "Selector cannot be null");

            return SelectAsyncImpl(sequence, UnboundedConcurrent, (item, idx, _) => selector(item, idx), token, scheduler);
        }

        [NotNull]
        public static IAsyncEnumerable<TResult> SelectEnumerbaleAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            [NotNull] Func<T, long, Task<TResult>> selector,
            CancellationToken token = default, [CanBeNull] TaskScheduler scheduler = null)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(selector, nameof(selector), "Selector cannot be null");

            return SelectEnumerableAsyncImpl(sequence, UnboundedConcurrent, (item, idx, _) => selector(item, idx), token, scheduler);
        }


        /// <summary>
        /// Concurrently invokes the specified asynchronous selector against all elements (and their indicies) in the sequence and asynchronously waits for completion
        /// of all queued work, returning the results. This method optionaly queues work using the specified Task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the source sequence</typeparam>
        /// <typeparam name="TResult">Type of item in the result sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="selector">The <see cref="Task{TResult}"/> returning func to apply to each element. This delegate receives as its parameters the item,
        /// its original index and the original <see cref="CancellationToken"/>.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// selector - Selector cannot be null
        /// </exception>
        [NotNull]
        public static Task<TResult[]> SelectAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null
        ) => SelectAsync(sequence, UnboundedConcurrent, selector, token, scheduler); // delegate signatures match so pass thru directly (not a composite delegate)


        /// <summary>
        /// Concurrently invokes the specified asynchronous selector against each element in the sequence and asynchronously waits for completion
        /// of all queued work, returning the results. The actions may be performed serially or concurrently (all at once or limited by <paramref name="degreesOfParallelism"/>),
        /// optionaly using the specified task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the source sequence</typeparam>
        /// <typeparam name="TResult">Type of item in the result sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="degreesOfParallelism">The number of tasks to process concurrently. When <c>0</c> all tasks will run concurrently. When <c>1</c> actions will be invoked
        /// serially (similar to a normal foreach loop) either A. on the current thread (when <paramref name="scheduler"/> is <c>null</c> or B. queued as a single task
        /// to the specified <paramref name="scheduler"/>. Otherwise, this value indicates the number of groups <paramref name="sequence"/> will be partitioned into, running
        /// no more than this number of tasks concurrently. </param>
        /// <param name="selector">The <see cref="Task{TResult}"/> returning func to apply to each element.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.
        /// When <c>null</c> and <paramref name="degreesOfParallelism"/> is <c>1</c>, work is queued beginning on the current thread.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// selector - Selector cannot be null
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// degreesOfParallelism - DOP must be greater than or equal to 0
        /// </exception>
        [NotNull]
        public static Task<TResult[]> SelectAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, Task<TResult>> selector,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {

            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(selector, nameof(selector), "Selector cannot be null");
            Requires.Range(degreesOfParallelism >= 0, nameof(degreesOfParallelism), "DOP must be greater than or equal to 0");

            return SelectAsyncImpl(sequence, degreesOfParallelism, (item, _, __) => selector(item), token, scheduler);
        }

        [NotNull]
        public static IAsyncEnumerable<TResult> SelectEnumerableAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, Task<TResult>> selector,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {

            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(selector, nameof(selector), "Selector cannot be null");
            Requires.Range(degreesOfParallelism >= 0, nameof(degreesOfParallelism), "DOP must be greater than or equal to 0");

            return SelectEnumerableAsyncImpl(sequence, degreesOfParallelism, (item, _, __) => selector(item), token, scheduler);
        }

        /// <summary>
        /// Concurrently invokes the specified asynchronous selector against each element (and its index) in the sequence and asynchronously waits for completion
        /// of all queued work, returning the results. The actions may be performed serially or concurrently (all at once or limited by <paramref name="degreesOfParallelism"/>),
        /// optionaly using the specified task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the source sequence</typeparam>
        /// <typeparam name="TResult">Type of item in the result sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="degreesOfParallelism">The number of tasks to process concurrently. When <c>0</c> all tasks will run concurrently. When <c>1</c> actions will be invoked
        /// serially (similar to a normal foreach loop) either A. on the current thread (when <paramref name="scheduler"/> is <c>null</c> or B. queued as a single task
        /// to the specified <paramref name="scheduler"/>. Otherwise, this value indicates the number of groups <paramref name="sequence"/> will be partitioned into, running
        /// no more than this number of tasks concurrently. </param>
        /// <param name="selector">The <see cref="Task{TResult}"/> returning func to apply to each element. This delegate receives as its arguments the item and its original index.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.
        /// When <c>null</c> and <paramref name="degreesOfParallelism"/> is <c>1</c>, work is queued beginning on the current thread.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// selector - Selector cannot be null
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// degreesOfParallelism - DOP must be greater than or equal to 0
        /// </exception>
        [NotNull]
        public static Task<TResult[]> SelectAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, Task<TResult>> selector,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {

            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(selector, nameof(selector), "Selector cannot be null");
            Requires.Range(degreesOfParallelism >= 0, nameof(degreesOfParallelism), "DOP must be greater than or equal to 0");

            return SelectAsyncImpl(sequence, degreesOfParallelism, (item, idx, _) => selector(item, idx), token, scheduler);
        }

        [NotNull]
        public static IAsyncEnumerable<TResult> SelectEnumerableAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, Task<TResult>> selector,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {

            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(selector, nameof(selector), "Selector cannot be null");
            Requires.Range(degreesOfParallelism >= 0, nameof(degreesOfParallelism), "DOP must be greater than or equal to 0");

            return SelectEnumerableAsyncImpl(sequence, degreesOfParallelism, (item, idx, _) => selector(item, idx), token, scheduler);
        }


        /// <summary>
        /// Concurrently invokes the specified asynchronous selector against each element (and its index) in the sequence and asynchronously waits for completion
        /// of all queued work, returning the results. The actions may be performed serially or concurrently (all at once or limited by <paramref name="degreesOfParallelism"/>),
        /// optionaly using the specified task scheduler.
        /// </summary>
        /// <typeparam name="T">Type of item in the source sequence</typeparam>
        /// <typeparam name="TResult">Type of item in the result sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="degreesOfParallelism">The number of tasks to process concurrently. When <c>0</c> all tasks will run concurrently. When <c>1</c> actions will be invoked
        /// serially (similar to a normal foreach loop) either A. on the current thread (when <paramref name="scheduler"/> is <c>null</c> or B. queued as a single task
        /// to the specified <paramref name="scheduler"/>. Otherwise, this value indicates the number of groups <paramref name="sequence"/> will be partitioned into, running
        /// no more than this number of tasks concurrently. </param>
        /// <param name="selector">The <see cref="Task{TResult}"/> returning func to apply to each element. This delegate receives as its parameters the item,
        /// its original index and the original <see cref="CancellationToken"/>.</param>
        /// <param name="token">An optional cancellation token</param>
        /// <param name="scheduler">An optional <see cref="TaskScheduler"/>. If no scheduler is provided, work will be queued using the default scheduler (<see cref="TaskScheduler.Default"/>.
        /// When <c>null</c> and <paramref name="degreesOfParallelism"/> is <c>1</c>, work is queued beginning on the current thread.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// selector - Selector cannot be null
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// degreesOfParallelism - DOP must be greater than or equal to 0
        /// </exception>
        [NotNull]
        public static Task<TResult[]> SelectAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {

            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(selector, nameof(selector), "Selector cannot be null");
            Requires.Range(degreesOfParallelism >= 0, nameof(degreesOfParallelism), "DOP must be greater than or equal to 0");

            return SelectAsyncImpl(sequence, degreesOfParallelism, selector, token, scheduler);
        }

        [NotNull]
        public static IAsyncEnumerable<TResult> SelectEnumerableAsync<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            CancellationToken token = default,
            [CanBeNull] TaskScheduler scheduler = null)
        {

            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(selector, nameof(selector), "Selector cannot be null");
            Requires.Range(degreesOfParallelism >= 0, nameof(degreesOfParallelism), "DOP must be greater than or equal to 0");

            return SelectEnumerableAsyncImpl(sequence, degreesOfParallelism, selector, token, scheduler);
        }


        [NotNull]
        private static Task ForEachAsyncImpl<[CanBeNull] T>(
            [NotNull, ItemCanBeNull, InstantHandle] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, CancellationToken, Task> body,
            CancellationToken token,
            [CanBeNull] TaskScheduler scheduler)
        {

            Debug.Assert(sequence != null, "sequence != null");
            Debug.Assert(body != null, "body != null");
            Debug.Assert(degreesOfParallelism >= 0, "degreesOfParallelism >= 0");

            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);

            return degreesOfParallelism switch
            {
                UnboundedConcurrent => UnboundedConcurrentIteration(sequence, body, token, scheduler),
                // passing "1" and no scheduler will process the collection on the
                // current thread, awaiting each call back in turn without launching new tasks
                Serial when scheduler is null => SerialIteration(sequence, body, token),
                _ => BoundedConcurrentIteration(sequence, degreesOfParallelism, body, token, scheduler)
            };
        }

        [NotNull]
        private static async Task SerialIteration<[CanBeNull] T>(
            [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
            [NotNull] Func<T, long, CancellationToken, Task> body,
            CancellationToken token)
        {
            await new SynchronizationContextRemover();

            if (CollectionUtils.AsReadOnlyList(sequence) is { } list)
            {
                for (var i = 0; i < list.Count; ++i)
                {
                    if (token.CanBeCanceled)
                        token.ThrowIfCancellationRequested();

                    await body(list[i], i, token);
                }
            }
            else
            {
                long index = 0;
                foreach (var item in sequence)
                {
                    if (token.CanBeCanceled)
                        token.ThrowIfCancellationRequested();

                    await body(item, index++, token);
                }
            }
        }

        [NotNull]
        private static Task UnboundedConcurrentIteration<[CanBeNull] T>(
            [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
            [NotNull] Func<T, long, CancellationToken, Task> body,
            CancellationToken token,
            [CanBeNull] TaskScheduler scheduler)
        {

            IEnumerable<Task> allTasks;

            if (CollectionUtils.AsReadOnlyList(sequence) is { } list)
            {
                var tasks = new Task[list.Count];
                for (var i = 0; i < tasks.Length; ++i)
                {
                    tasks[i] = CreateIterationTask(list[i], i, token, body, scheduler);
                }

                allTasks = tasks;
            }
            else
            {
                allTasks = sequence.Select((item, index) =>
                    CreateIterationTask(item, index, token, body, scheduler)
                );
            }

            return Task.WhenAll(allTasks).WaitAsync(token);
        }

        [NotNull]
        private static Task CreateIterationTask<[CanBeNull] T>([CanBeNull] T item, long index, CancellationToken token,
            [NotNull] Func<T, long, CancellationToken, Task> body, [CanBeNull] TaskScheduler scheduler)
        {
            var stateIn = new IterationState<T>(item, index, token, body);
            return scheduler is null
                ? Task.Run(() => stateIn.Invoke(), token)
                // only use Task.Factory.StartNew when ABSOLUTELY necessary
                : Task.Factory.StartNew(state => ((IterationState<T>)state).Invoke(), stateIn, token,
                    DenyAttach, scheduler).Unwrap();
        }

        [NotNull]
        private static Task BoundedConcurrentIteration<[CanBeNull] T>(
            [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, CancellationToken, Task> body,
            CancellationToken token,
            [CanBeNull] TaskScheduler scheduler)
        {

            // preserve the original index.
            // Call ToList to prevent outside enumeration from breaking (if left in fire-forget mode)
            var seqWithIndex = sequence.Select((item, idx) => (item, idx)).ToList();
            var partitions = Partitioner.Create(seqWithIndex).GetPartitions(degreesOfParallelism);

            var tasks = new Task[partitions.Count];
            for (var i = 0; i < partitions.Count; ++i)
            {
                var stateIn = new PartitionState<T>(partitions[i], body, token);

                // only use Task.Factory.StartNew when ABSOLUTELY necessary
                tasks[i] = scheduler is null
                    ? Task.Run(() => stateIn.Invoke(), token)
                    : Task.Factory.StartNew(state => ((PartitionState<T>) state).Invoke(), stateIn, token,
                        DenyAttach, scheduler).Unwrap();
            }

            return Task.WhenAll(tasks).WaitAsync(token);
        }


        [NotNull]
        private static Task<TResult[]> SelectAsyncImpl<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] this IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            CancellationToken token,
            [CanBeNull] TaskScheduler scheduler)
        {

            Debug.Assert(sequence != null, "sequence != null");
            Debug.Assert(selector != null, "body != null");
            Debug.Assert(degreesOfParallelism >= 0, "degreesOfParallelism >= 0");

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (token.IsCancellationRequested)
                return Task.FromCanceled<TResult[]>(token);

            return degreesOfParallelism switch
            {
                UnboundedConcurrent => UnboundedConcurrentSelect(sequence, selector, token, scheduler),
                // passing "1" and no scheduler will process the collection on the
                // current thread, awaiting each call back in turn without launching new tasks
                Serial when scheduler is null => SerialSelect(sequence, selector, token),
                _ => BoundedConcurrentSelect(sequence, degreesOfParallelism, selector, token, scheduler)
            };
        }

        private static IAsyncEnumerable<TResult> SelectEnumerableAsyncImpl<T, TResult>(
            this IEnumerable<T> sequence, int degreesOfParallelism,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector, CancellationToken token, [CanBeNull] TaskScheduler scheduler)
        {
            token.ThrowIfCancellationRequested();

            return degreesOfParallelism switch
            {
                UnboundedConcurrent => UnboundedConcurrentEnumerableSelect(sequence, selector, token, scheduler),
                Serial when scheduler is null => SerialEnumerableSelect(sequence, selector, token),
                _ => BoundedConcurrentEnumableSelect(sequence, degreesOfParallelism, selector, token, scheduler)
            };
        }

        private static async IAsyncEnumerable<TResult> SerialEnumerableSelect<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            [EnumeratorCancellation] CancellationToken token)
        {
            await new SynchronizationContextRemover();

            if (CollectionUtils.AsReadOnlyList(sequence) is { } list)
            {
                for (var i = 0; i < list.Count; ++i)
                {
                    if (token.CanBeCanceled)
                        token.ThrowIfCancellationRequested();

                    yield return await selector(list[i], i, token);
                }
            }
            else
            {
                long index = 0;
                foreach (var item in sequence)
                {
                    if (token.CanBeCanceled)
                        token.ThrowIfCancellationRequested();

                    yield return await selector(item, index++, token);
                }
            }
        }


        [NotNull]
        private static async Task<TResult[]> SerialSelect<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            CancellationToken token)
        {
            await new SynchronizationContextRemover();

            if (CollectionUtils.AsReadOnlyList(sequence) is { } list)
            {
                var output = new TResult[list.Count];

                for (var i = 0; i < list.Count; ++i)
                {
                    if (token.CanBeCanceled)
                        token.ThrowIfCancellationRequested();

                    output[i] = await selector(list[i], i, token);
                }

                return output;
            }
            else
            {
                var output = new List<TResult>();
                long index = 0;
                foreach (var item in sequence)
                {
                    if (token.CanBeCanceled)
                        token.ThrowIfCancellationRequested();

                    output.Add(await selector(item, index++, token));
                }

                return output.ToArray();
            }
        }

        private static async IAsyncEnumerable<TResult> UnboundedConcurrentEnumerableSelect<T, TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            [EnumeratorCancellation] CancellationToken token,
            [CanBeNull] TaskScheduler scheduler)
        {
            var result = await UnboundedConcurrentSelect(sequence, selector, token, scheduler);
            foreach (var r in result)
                yield return r;
        }

        [NotNull]
        private static Task<TResult[]> UnboundedConcurrentSelect<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            CancellationToken token,
            [CanBeNull] TaskScheduler scheduler)
        {

            IEnumerable<Task<TResult>> allTasks;

            if (CollectionUtils.AsReadOnlyList(sequence) is { } list)
            {
                var tasks = new Task<TResult>[list.Count];
                for (var i = 0; i < tasks.Length; ++i)
                {
                    tasks[i] = CreateIterationTask(list[i], i, token, selector, scheduler);
                }

                allTasks = tasks;
            }
            else
            {
                allTasks = sequence.Select((item, index) => CreateIterationTask(item, index, token, selector, scheduler));
            }

            return Task.WhenAll(allTasks).WaitAsync(token);
        }

        [NotNull]
        private static Task<TResult> CreateIterationTask<[CanBeNull] T, [CanBeNull] TResult>([CanBeNull] T item, long index, CancellationToken token,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> body, [CanBeNull] TaskScheduler scheduler)
        {
            var stateIn = new SelectState<T, TResult>(item, index, token, body);
            return scheduler is null
                    ? Task.Run(() => stateIn.Invoke(), token)
                    // only use Task.Factory.StartNew when ABSOLUTELY necessary
                    : Task.Factory.StartNew(state => ((SelectState<T, TResult>)state).Invoke(), stateIn, token,
                                            DenyAttach, scheduler).Unwrap();
        }


        private static async IAsyncEnumerable<TResult> BoundedConcurrentEnumableSelect<T, TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            [EnumeratorCancellation] CancellationToken token,
            [CanBeNull] TaskScheduler scheduler)
        {
            // BUG? Piggy-backing on the BoundedConcurrentSelect forces output in order
            // BUG-> correct! because we are just returning the tasks in the original order
            // BUG-> Need option for "in order" returns using TaskCompletionSource
            var result = await BoundedConcurrentSelect(sequence, degreesOfParallelism, selector, token, scheduler);
            foreach (var r in result)
                yield return r;

        }


        [NotNull]
        private static Task<TResult[]> InOrderBoundedConcurrentSelect<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            CancellationToken token,
            [CanBeNull] TaskScheduler scheduler)
        {

            scheduler ??= TaskScheduler.Default;

            // partitioned is harder/bit more of a mess when we need a return value
            // delegate to the runtime's implementation. Use ConcurrentExclusiveSchedulePair
            // to *wrap* the passed in scheduler and then use IT with a max DOP
            var pair = new ConcurrentExclusiveSchedulerPair(scheduler, degreesOfParallelism);
            var concurrentScheduler = pair.ConcurrentScheduler;

            // TODO -> this is just some dummy work as a proof of concept
            // TODO -> clean this up
            var count = sequence.Count();

            var tcs = Enumerable.Range(0, count).Select(c => new TaskCompletionSource<TResult>()).ToArray();
            int index = -1;

            _ = sequence.Select((x, i) => Task.Factory
                                                        .StartNew(o =>
                                                                  {
                                                                      var r = ((SelectState<T, TResult>) o).Invoke();

                                                                      r.ContinueWith(
                                                                          s => tcs[Interlocked.Increment(ref index)].SetResult(s.Result),
                                                                          TaskContinuationOptions.OnlyOnRanToCompletion);

                                                                      return r;
                                                                  },
                                                                  new SelectState<T, TResult>(x, i, token, selector), token,
                                                                  DenyAttach, concurrentScheduler)
                                                        .Unwrap()).ToArray();

            return Task.WhenAll(tcs.Select(d => d.Task)).WaitAsync(token);
        }

        [NotNull]
        private static Task<TResult[]> BoundedConcurrentSelect<[CanBeNull] T, [CanBeNull] TResult>(
            [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
            int degreesOfParallelism,
            [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
            CancellationToken token,
            [CanBeNull] TaskScheduler scheduler)
        {

            scheduler ??= TaskScheduler.Default;

            // partitioned is harder/bit more of a mess when we need a return value
            // delegate to the runtime's implementation. Use ConcurrentExclusiveSchedulePair
            // to *wrap* the passed in scheduler and then use IT with a max DOP
            var pair = new ConcurrentExclusiveSchedulerPair(scheduler, degreesOfParallelism);
            var concurrentScheduler = pair.ConcurrentScheduler;

            var results = sequence.Select((x, i) => Task.Factory
                                                        .StartNew(o => ((SelectState<T, TResult>) o).Invoke(),
                                                                  new SelectState<T, TResult>(x, i, token, selector), token,
                                                                  DenyAttach, concurrentScheduler)
                                                        .Unwrap());

            return Task.WhenAll(results).WaitAsync(token);
        }

#if NETCOREAPP3_0||NETCOREAPP3_1 // TODO
        private const MethodImplOptions MethodOps = MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining;
#else
        private const MethodImplOptions MethodOps = MethodImplOptions.AggressiveInlining;
#endif

        [DebuggerStepThrough]
        private struct IterationState<T>
        {
            private readonly long _index;
            private readonly T _item;
            private readonly CancellationToken _token;
            private readonly Func<T, long, CancellationToken, Task> _body;

            [MethodImpl(MethodOps)]
            public readonly async Task Invoke()
            {
                await new SynchronizationContextRemover();

                if (_token.CanBeCanceled)
                    _token.ThrowIfCancellationRequested();

                await _body(_item, _index, _token);
            }

            public IterationState(T item, long idx, CancellationToken token,
                Func<T, long, CancellationToken, Task> body)
            {
                _item = item;
                _index = idx;
                _token = token;
                _body = body;
            }
        }

        [DebuggerStepThrough]
        private struct SelectState<T, TResult>
        {
            private readonly long _index;
            private readonly T _item;
            private readonly CancellationToken _token;
            private readonly Func<T, long, CancellationToken, Task<TResult>> _body;

            [MethodImpl(MethodOps)]
            public readonly async Task<TResult> Invoke()
            {
                await new SynchronizationContextRemover();

                if (_token.CanBeCanceled)
                    _token.ThrowIfCancellationRequested();

                return await _body(_item, _index, _token);
            }

            public SelectState(T item, long idx, CancellationToken token,
                Func<T, long, CancellationToken, Task<TResult>> body)
            {
                _item = item;
                _index = idx;
                _token = token;
                _body = body;
            }
        }

        [DebuggerStepThrough]
        private struct PartitionState<T>
        {
            private readonly IEnumerator<(T, int)> _enumerator;
            private readonly CancellationToken _token;
            private readonly Func<T, long, CancellationToken, Task> _body;

            public PartitionState(IEnumerator<(T, int)> enumerator, Func<T, long, CancellationToken, Task> body,
                CancellationToken token)
            {
                _enumerator = enumerator;
                _token = token;
                _body = body;
            }

            [MethodImpl(MethodOps)]
            public readonly async Task Invoke()
            {
                await new SynchronizationContextRemover();

                var enumerator = _enumerator;
                using (enumerator)
                {
                    while (enumerator.MoveNext())
                    {
                        if (_token.CanBeCanceled)
                            _token.ThrowIfCancellationRequested();

                        var (item, idx) = enumerator.Current;
                        await _body(item, idx, _token);
                    }
                }
            }

        }
    }

    #region Alternative for Select Partitioned
    //[NotNull]
    //private static async Task<TResult[]> BoundedConcurrentSelect<[CanBeNull] T, [CanBeNull] TResult>(
    //    [NotNull, ItemCanBeNull, InstantHandle] IEnumerable<T> sequence,
    //    int degreesOfParallelism,
    //    [NotNull] Func<T, long, CancellationToken, Task<TResult>> selector,
    //    CancellationToken token,
    //    [CanBeNull] TaskScheduler scheduler)
    //{

    //    // preserve the original index.
    //    // Call ToList to prevent outside enumeration from breaking (if left in fire-forget mode)
    //    var seqWithIndex = sequence.Select((item, idx) => (item, idx)).ToList();

    //    var partitions = Partitioner.Create(seqWithIndex).GetPartitions(degreesOfParallelism);

    //    var tasks = new Task[partitions.Count];

    //    var results = new TResult[seqWithIndex.Count];

    //    for (var i = 0; i < partitions.Count; ++i)
    //    {
    //        var stateIn = new PartitionState<T>(partitions[i], selector, token);

    //        // only use Task.Factory.StartNew when ABSOLUTELY necessary
    //        tasks[i] = scheduler is null
    //                ? Task.Run(() => stateIn.Invoke(results), token)
    //                : Task.Factory.StartNew(state => ((PartitionState<T>) state).Invoke(results), stateIn, token,
    //                                        DenyAttach, scheduler).Unwrap();
    //    }

    //    await Task.WhenAll(tasks).WaitAsync(token);

    //    return results;
    //}
    //[MethodImpl(MethodOps)]
    //public readonly async Task Invoke<TResult>(IList<TResult> results)
    //{
    //    await new SynchronizationContextRemover();

    //    var enumerator = _enumerator;
    //    using (enumerator)
    //    {
    //        while (enumerator.MoveNext())
    //        {
    //            if (_token.CanBeCanceled)
    //                _token.ThrowIfCancellationRequested();

    //            var (item, idx) = enumerator.Current;
    //            results[idx] = await ((Func<T, long, CancellationToken, Task<TResult>>) _body)(item, idx, _token);

    //        }
    //    }
    //}
    #endregion

}