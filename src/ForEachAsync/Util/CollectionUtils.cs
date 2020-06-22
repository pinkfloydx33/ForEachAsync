using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace Floydcom.ForEachAsync.Util
{
    internal static class CollectionUtils
    {
        [CanBeNull, ItemCanBeNull]
        public static IReadOnlyList<T> AsReadOnlyList<[CanBeNull] T>([NoEnumeration, CanBeNull] IEnumerable<T> sequence)
        {
            return sequence switch
            {
                IReadOnlyList<T> roList => roList,
                IList<T> list => new Wrapper<T>(list),
                _ => null
            };
        }

        private sealed class Wrapper<T> : IReadOnlyList<T>
        {

            private readonly IList<T> _inner;

            public Wrapper([NotNull] IList<T> list)
            {
                Debug.Assert(list != null, "list != null");
                _inner = list;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
            
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _inner.Count;
            }

            public T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _inner[index];
            }
        }
    }
}