using System;
using System.Collections.Generic;
using System.Diagnostics;
using Floydcom.ForEachAsync.Util;
using JetBrains.Annotations;

namespace Floydcom.ForEachAsync
{

    /// <summary>
    /// Class EnumerableExtensions. Extensions methods for operating on a collections of items
    /// </summary>
    [PublicAPI]
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Batches a sequence into batches of a particular size
        /// </summary>
        /// <typeparam name="T">The type of collection item</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns>IEnumerable&lt;List&lt;T&gt;&gt;.</returns>
        /// <exception cref="ArgumentNullException">Sequence cannot be null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Batch size must be greater than zero</exception>        
        [NotNull, ItemNotNull]
        public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> sequence, int batchSize)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.Range(batchSize > 0, nameof(batchSize), "Batch Size must be greater than zero");

            return Iterator();

            IEnumerable<List<T>> Iterator()
            {
                var batch = new List<T>(batchSize);
                foreach (var item in sequence)
                {
                    batch.Add(item);
                    // when we've accumulated enough in the batch, send it out  
                    if (batch.Count >= batchSize)
                    {
                        yield return batch;
                        batch = new List<T>(batchSize);
                    }
                }

                if (batch.Count > 0)
                {
                    yield return batch;
                }
            }
        }

        /// <summary>
        /// Performs the specified action for each element in the sequence
        /// </summary>
        /// <typeparam name="T">Type of item in the sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="action">The action to perform.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// action - Action cannot be null
        /// </exception>
        public static void ForEach<[CanBeNull] T>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence,
            [NotNull, InstantHandle] Action<T> action)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(action, nameof(action), "Action cannot be null");

            ForEachImpl(sequence, (item, _) => action(item));
        }

        /// <summary>
        /// Performs the specified action for each element (and its index) in the sequence
        /// </summary>
        /// <typeparam name="T">Type of item in the sequence</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="action">The action to perform, receiving as its parameters the item and its index.</param>
        /// <exception cref="ArgumentNullException">
        /// sequence - Sequence cannot be null
        /// or
        /// action - Action cannot be null
        /// </exception>
        public static void ForEach<[CanBeNull] T>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence, 
            [NotNull, InstantHandle] Action<T, long> action)
        {
            Requires.NotNull(sequence, nameof(sequence), "Sequence cannot be null");
            Requires.NotNull(action, nameof(action), "Action cannot be null");

            ForEachImpl(sequence, action);
        }

        private static void ForEachImpl<[CanBeNull] T>(
            [NotNull, InstantHandle, ItemCanBeNull] this IEnumerable<T> sequence, 
            [NotNull, InstantHandle] Action<T, long> action)
        {

            Debug.Assert(sequence != null, "sequence != null");
            Debug.Assert(action != null, "action != null");

            if (CollectionUtils.AsReadOnlyList(sequence) is {} list)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < list.Count; ++i)
                    action(list[i], i);
            }
            else
            {
                long idx = 0;
                foreach (var item in sequence)
                    action(item, idx++);
            }
        }
    }
}