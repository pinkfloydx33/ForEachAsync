using System.Collections;
using System.Collections.Generic;
using Floydcom.ForEachAsync.Util;
using Moq;
using Xunit;
// ReSharper disable PossibleNullReferenceException
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForCanBeConvertedToForeach

namespace Floydcom.ForEachAsync.Tests.Util
{

    public class CollectionUtilsTests
    {
        [Fact]
        public void CollectionUtils_AsReadOnlyList_ReturnsNull_WhenNull()
        {
            var result = CollectionUtils.AsReadOnlyList<int>(null);
            Assert.Null(result);
        }

        [Fact]
        public void CollectionUtils_AsReadOnlyList_ReturnsNull_WhenNotListOrReadOnlyList()
        {
            var dict = new Dictionary<int, int> { [1] = 2, [3] = 4 };
            var result = CollectionUtils.AsReadOnlyList(dict);
            Assert.Null(result);
        }

        [Fact]
        public void CollectionUtils_AsReadOnlyList_ReturnsOriginal_WhenAlreadyReadOnlyList()
        {
            var list = new List<int> { 1, 2, 3 };
            var result = CollectionUtils.AsReadOnlyList(list);
            Assert.NotNull(result);
            Assert.Same(list, result);
        }

        [Fact]
        public void CollectionUtils_AsReadOnlyList_ReturnsWrapper_WhenListButNotReadOnly()
        {
            var list = new Mock<IList<int>>();
            var result = CollectionUtils.AsReadOnlyList(list.Object);
            Assert.NotNull(result);
            Assert.NotSame(list, result);
            
            Assert.Equal("Wrapper`1", result.GetType().Name);
        }

        [Fact]
        public void CollectionUtils_Wrapper_Indexer_OperatesOverWrappedList()
        {
            var backing = new List<int> { 1, 2, 3 };

            var list = new Mock<IList<int>>();
            list.SetupGet(c => c.Count)
                .Returns(backing.Count);
            list.Setup(c => c[It.IsAny<int>()])
                .Returns((int index) => backing[index]);

            var result = CollectionUtils.AsReadOnlyList(list.Object);

            Assert.NotNull(result);
            Assert.NotSame(list, result);

            var newList = new List<int>();
            
            for (var i = 0; i < result.Count; ++i)
                newList.Add(result[i]);
          
            Assert.Equal(backing, newList, EqualityComparer<int>.Default);

            list.Verify(c => c[0], Times.Once);
            list.Verify(c => c[1], Times.Once);
            list.Verify(c => c[2], Times.Once);
            list.Verify(c => c.Count, Times.Exactly(backing.Count + 1));

        }

        [Fact]
        public void CollectionUtils_Wrapper_Enumerator_OperatesOverWrappedList()
        {
            var backing = new List<int> { 1, 2, 3 };

            var list = new Mock<IList<int>>();
            list.Setup(c => c.GetEnumerator())
                .Returns(() => backing.GetEnumerator());
                
            var result = CollectionUtils.AsReadOnlyList(list.Object);

            Assert.NotNull(result);
            Assert.NotSame(list, result);

            var newList = new List<int>();
            foreach (var i in result)
                newList.Add(i);

            Assert.Equal(newList, backing, EqualityComparer<int>.Default);

            list.Verify(c => c[It.IsAny<int>()], Times.Never);
            list.Verify(c => c.GetEnumerator(), Times.Once);

        }

        [Fact]
        public void CollectionUtils_Wrapper_IEnumerableEnumerator_OperatesOverWrappedList()
        {
            var backing = new List<int> { 1, 2, 3 };

            var list = new Mock<IList<int>>();
            list.Setup(c => c.GetEnumerator())
                .Returns(() => backing.GetEnumerator());
                
            var result = CollectionUtils.AsReadOnlyList(list.Object);

            Assert.NotNull(result);
            Assert.NotSame(list, result);

            var newList = new List<int>();

            foreach (var i in (IEnumerable) result)
                newList.Add((int)i);

            Assert.Equal(newList, backing, EqualityComparer<int>.Default);

            list.Verify(c => c[It.IsAny<int>()], Times.Never);
            list.Verify(c => c.GetEnumerator(), Times.Once);

        }
    }
}
