using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
// ReSharper disable AssignNullToNotNullAttribute

namespace Floydcom.ForEachAsync.Tests
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void EnumerableExtensions_ForEach_ThrowsOnNullSequence()
        {
            IEnumerable<int> nullSequence = null;

            Assert.Throws<ArgumentNullException>(() => nullSequence.ForEach(e => { }));
            Assert.Throws<ArgumentNullException>(() => nullSequence.ForEach((e, i) => { }));
        }

        [Fact]
        public void EnumerableExtensions_ForEach_ThrowsOnNullAction()
        {
            IEnumerable<int> sequence = new[] { 1, 2, 3 };

            Assert.Throws<ArgumentNullException>(() => sequence.ForEach((Action<int>)null));
            Assert.Throws<ArgumentNullException>(() => sequence.ForEach((Action<int, long>) null));
        }

        [Fact]
        public void EnumerableExtensions_ForEach_SucceedsOnEmptyList()
        {
            IEnumerable<int> list = new List<int>();
            var act1 = new Mock<Action<int>>();
            var act2 = new Mock<Action<int, long>>();

            list.ForEach(act1.Object);
            list.ForEach(act2.Object);

            act1.Verify(c => c.Invoke(It.IsAny<int>()), Times.Never);
            act2.Verify(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public void EnumerableExtensions_ForEach_SucceedsOnEmptyEnumerable()
        {
            var list = new Mock<IEnumerable<int>>();
            var @enum = new Mock<IEnumerator<int>>();

            @enum.Setup(c => c.MoveNext())
                .Returns(false);

            @enum.SetupGet(c => c.Current)
                .Throws(new Exception("Should not get here"));

            list.Setup(c => c.GetEnumerator())
                .Returns(() => @enum.Object);

            var act1 = new Mock<Action<int>>();
            var act2 = new Mock<Action<int, long>>();

            list.Object.ForEach(act1.Object);
            list.Object.ForEach(act2.Object);

            act1.Verify(c => c.Invoke(It.IsAny<int>()), Times.Never);
            act2.Verify(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public void EnumerableExtensions_ForEach_IListDoesNotInvokeEnumerator()
        {
            var seq = new Mock<IList<int>>();
            seq.SetupGet(c => c.Count).Returns(1);
            seq.SetupGet(c => c[0]).Returns(33);

            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                .Throws(new Exception("Enumerator should not be called"));

            seq.Object.ForEach(item =>
            {
                Assert.Equal(33, item);
            });

            seq.Object.ForEach((item, index) =>
            {
                Assert.Equal(33, item);
                Assert.Equal(0, index);
            });

            seq.VerifyGet(c => c.Count, Times.Exactly(4)); // twice each 
            seq.VerifyGet(c => c[0], Times.Exactly(2)); // once each

            iEnum.Verify(c => c.GetEnumerator(), Times.Never);

        }

        [Fact]
        public void EnumerableExtensions_ForEach_NotIListInvokesEnumerator()
        {
            var backingList = new List<int> { 42, 33 };

            var iEnum = new Mock<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                .Returns(() => backingList.GetEnumerator());

            var act1 = new Mock<Action<int>>();
            var act2 = new Mock<Action<int, long>>();
            
            iEnum.Object.ForEach(act1.Object);
            iEnum.Object.ForEach(act2.Object);

            iEnum.Verify(c => c.GetEnumerator(), Times.Exactly(2)); // once each

            // verify delegate invocations
            act1.Verify(c => c.Invoke(42), Times.Once);
            act1.Verify(c => c.Invoke(33), Times.Once);

            act2.Verify(c => c.Invoke(42, 0), Times.Once);
            act2.Verify(c => c.Invoke(33, 1), Times.Once);

        }

        [Fact]
        public void EnumerableExtensions_BatchEnumerable_ThrowsOnNullCollection()
        {
            IEnumerable<int> nullEnumerable = null;
            Assert.Throws<ArgumentNullException>(() => nullEnumerable.Batch(1));
            Assert.Throws<ArgumentNullException>(() => nullEnumerable.Batch(50));
        }

        [Fact]
        public void EnumerableExtensions_BatchEnumerable_ThrowsOnInvalidBatchSize()
        {
            var list = new[] { 1, 2, 3 };
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Batch(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Batch(-100));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Batch(-1));
        }

        [Fact]
        public void EnumerableExtensions_BatchEnumerable_ReturnsEmptyList_ForEmptyCollection()
        {
            var list = Array.Empty<int>();

            var result = list.Batch(1);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void EnumerableExtensions_BatchEnumerable_Returns_BatchesOfExpectedSize()
        {
            var list = Enumerable.Range(0, 50);
            var result = list.Batch(10);
            
            Assert.NotNull(result);

            var resultAsList = result.ToList();

            Assert.NotEmpty(resultAsList);

            Assert.Equal(5, resultAsList.Count);
            Assert.All(resultAsList, s => Assert.Equal(10, s.Count));

        }

        [Fact]
        public void EnumerableExtensions_BatchEnumerable_ReturnsFinalBatch_IfNotEvenlyDivisible()
        {
            var list = Enumerable.Range(0, 10);
            var result = list.Batch(4);
            
            Assert.NotNull(result);

            var resultAsList = result.ToList();

            Assert.NotEmpty(resultAsList);

            Assert.Equal(3, resultAsList.Count);

            Assert.Equal(4, resultAsList[0].Count);
            Assert.Equal(4, resultAsList[1].Count);
            Assert.Equal(2, resultAsList[2].Count);
        }

        [Fact]
        public void EnumerableExtensions_BatchEnumerable_ReturnsOneBatch_IfBatchSizeIsGreaterThanEnumerableSize()
        {
            var list = Enumerable.Range(0, 10);
            var result = list.Batch(20);
            
            Assert.NotNull(result);

            var resultAsList = result.ToList();

            Assert.NotEmpty(resultAsList);
            Assert.Single(resultAsList);
            Assert.Equal(10, resultAsList[0].Count);
        }
    }
}
