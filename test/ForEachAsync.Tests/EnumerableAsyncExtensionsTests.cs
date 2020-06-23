using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable ExpressionIsAlwaysNull

// ReSharper disable AssignNullToNotNullAttribute

namespace Floydcom.ForEachAsync.Tests
{
    public class EnumerableAsyncExtensionsTests
    {
        private readonly ITestOutputHelper _output;
        public EnumerableAsyncExtensionsTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task EnumerableAsyncExtensions_ForEachAsync_ThrowsOnNullSequence()
        {
            IEnumerable<int> nullSequence = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.ForEachAsync((e, i, c) => Task.CompletedTask));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.ForEachAsync((e, i) => Task.CompletedTask));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.ForEachAsync(e => Task.CompletedTask));

            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.ForEachAsync(1, (e, i, c) => Task.CompletedTask));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.ForEachAsync(1, (e, i) => Task.CompletedTask));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.ForEachAsync(1, e => Task.CompletedTask));
            
        }

        [Fact]
        public async Task EnumerableAsyncExtensions_ForEachAsync_ThrowsOnNullAction()
        {
            IEnumerable<int> sequence = new[] { 1, 2, 3 };

            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.ForEachAsync((Func<int, long, CancellationToken, Task>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.ForEachAsync((Func<int, long, Task>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.ForEachAsync((Func<int, Task>)null));
            

            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.ForEachAsync(1, (Func<int, long, CancellationToken, Task>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.ForEachAsync(1, (Func<int, long, Task>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.ForEachAsync(1, (Func<int, Task>)null));

        }

        [Fact]
        public async Task EnumerableAsyncExtensions_ForEachAsync_ThrowsOnNegativeDoP()
        {
            IEnumerable<int> sequence = new[] { 1, 2, 3 };

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sequence.ForEachAsync(-1, (e, i, c) => Task.CompletedTask));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sequence.ForEachAsync(-2, (e, i) => Task.CompletedTask));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sequence.ForEachAsync(-3, e => Task.CompletedTask));

        }

        [Fact]
        public async Task EnumerableAsyncExtensions_SelectAsync_ThrowsOnNullSequence()
        {
            IEnumerable<int> nullSequence = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.SelectAsync((e, i, c) => Task.FromResult(1)));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.SelectAsync((e, i) => Task.FromResult(1)));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.SelectAsync(e => Task.FromResult(1)));

            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.SelectAsync(1, (e, i, c) => Task.FromResult(1)));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.SelectAsync(1, (e, i) => Task.FromResult(1)));
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullSequence.SelectAsync(1, e => Task.FromResult(1)));
            
        }

        [Fact]
        public async Task EnumerableAsyncExtensions_SelectAsync_ThrowsOnNullAction()
        {
            IEnumerable<int> sequence = new[] { 1, 2, 3 };

            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.SelectAsync((Func<int, long, CancellationToken, Task<int>>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.SelectAsync((Func<int, long, Task<int>>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.SelectAsync((Func<int, Task<int>>)null));
            

            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.SelectAsync(1, (Func<int, long, CancellationToken, Task<int>>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.SelectAsync(1, (Func<int, long, Task<int>>)null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => sequence.SelectAsync(1, (Func<int, Task<int>>)null));

        }

        [Fact]
        public async Task EnumerableAsyncExtensions_SelectAsync_ThrowsOnNegativeDoP()
        {
            IEnumerable<int> sequence = new[] { 1, 2, 3 };

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sequence.SelectAsync(-1, (e, i, c) => Task.FromResult(1)));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sequence.SelectAsync(-2, (e, i) => Task.FromResult(1)));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sequence.SelectAsync(-3, e => Task.FromResult(1)));

        }


        [Fact]
        public void EnumerableAsyncExtensions_ForEachAsync_CancelledTokenReturnsCancelledTask()
        {
            IEnumerable<int> sequence = new[] { 1, 2, 3 };
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.All(new[]
                {
                    sequence.ForEachAsync((e, i, c) => Task.CompletedTask, cts.Token),
                    sequence.ForEachAsync((e, i) => Task.CompletedTask, cts.Token),
                    sequence.ForEachAsync(e => Task.CompletedTask, cts.Token),

                    sequence.ForEachAsync(1, (e, i, c) => Task.CompletedTask, cts.Token),
                    sequence.ForEachAsync(2, (e, i) => Task.CompletedTask, cts.Token),
                    sequence.ForEachAsync(3, e => Task.CompletedTask, cts.Token)
                },
                task => Assert.True(task.IsCanceled)
            );

        }

        [Fact]
        public void EnumerableAsyncExtensions_SelectAsync_CancelledTokenReturnsCancelledTask()
        {
            IEnumerable<int> sequence = new[] { 1, 2, 3 };
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.All(new[]
                       {
                           sequence.SelectAsync((e, i, c) => Task.FromResult(1), cts.Token),
                           sequence.SelectAsync((e, i) => Task.FromResult(1), cts.Token),
                           sequence.SelectAsync(e => Task.FromResult(1), cts.Token),
                                    
                           sequence.SelectAsync(1, (e, i, c) => Task.FromResult(1), cts.Token),
                           sequence.SelectAsync(2, (e, i) => Task.FromResult(1), cts.Token),
                           sequence.SelectAsync(3, e => Task.FromResult(1), cts.Token)
                       },
                       task => Assert.True(task.IsCanceled)
            );

        }


        [Fact]
        public async Task EnumerableAsyncExtensions_ForEachAsync_Partitioned_IListInvokesEnumerator()
        {
            var backer = new List<int> { 33, 42 };

            var seq = new Mock<IList<int>>();
            seq.SetupGet(c => c.Count).Returns(backer.Count);
            
            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                .Returns(() => backer.GetEnumerator());
            
            var (act1, act2, act3) = CreateMockCompletedForEachTaskDelegates();

            await iEnum.Object.ForEachAsync(2, act1.Object); // partitioned
            await iEnum.Object.ForEachAsync(2, act2.Object);
            await iEnum.Object.ForEachAsync(2, act3.Object);

            iEnum.Verify(c => c.GetEnumerator(), Times.Exactly(3)); // once each

            seq.VerifyGet(c => c.Count, Times.Never); // 2 per serial iteration, 1 per unbounded, 0 otherwise
            
            // verify delegate invocations
            act1.Verify(c => c.Invoke(33, 0, default), Times.Once);
            act1.Verify(c => c.Invoke(42, 1, default), Times.Once);
            act2.Verify(c => c.Invoke(33, 0), Times.Once);
            act2.Verify(c => c.Invoke(42, 1), Times.Once);
            act3.Verify(c => c.Invoke(33), Times.Once);
            act3.Verify(c => c.Invoke(42), Times.Once);

        }

        [Fact]
        public async Task EnumerableAsyncExtensions_SelectAsync_Partitioned_IListInvokesEnumerator()
        {
            var backer = new List<int> { 33, 42 };

            var seq = new Mock<IList<int>>();
            seq.SetupGet(c => c.Count).Returns(backer.Count);
            
            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                 .Returns(() => backer.GetEnumerator());
            
            var (act1, act2, act3) = CreateMockCompletedSelectTaskDelegates<int>();

            await iEnum.Object.SelectAsync(2, act1.Object); // partitioned
            await iEnum.Object.SelectAsync(2, act2.Object);
            await iEnum.Object.SelectAsync(2, act3.Object);

            iEnum.Verify(c => c.GetEnumerator(), Times.Exactly(3)); // once each

            seq.VerifyGet(c => c.Count, Times.Never); // 2 per serial iteration, 1 per unbounded, 0 otherwise
            
            // verify delegate invocations
            act1.Verify(c => c.Invoke(33, 0, default), Times.Once);
            act1.Verify(c => c.Invoke(42, 1, default), Times.Once);
            act2.Verify(c => c.Invoke(33, 0), Times.Once);
            act2.Verify(c => c.Invoke(42, 1), Times.Once);
            act3.Verify(c => c.Invoke(33), Times.Once);
            act3.Verify(c => c.Invoke(42), Times.Once);

        }

        [Fact]
        public async Task EnumerableAsyncExtensions_ForEachAsync_Partitioned_IReadOnlyListInvokesEnumerator()
        {
            var backer = new List<int> { 33, 42 };

            var seq = new Mock<IReadOnlyList<int>>();
            seq.SetupGet(c => c.Count).Returns(backer.Count);
            
            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                .Returns(() => backer.GetEnumerator());
            
            var (act1, act2, act3) = CreateMockCompletedForEachTaskDelegates();

            await iEnum.Object.ForEachAsync(2, act1.Object); // partitioned
            await iEnum.Object.ForEachAsync(2, act2.Object);
            await iEnum.Object.ForEachAsync(2, act3.Object);

            iEnum.Verify(c => c.GetEnumerator(), Times.Exactly(3)); // once each

            seq.VerifyGet(c => c.Count, Times.Never); // 2 per serial iteration, 1 per unbounded, 0 otherwise
            
            // verify delegate invocations
            act1.Verify(c => c.Invoke(33, 0, default), Times.Once);
            act1.Verify(c => c.Invoke(42, 1, default), Times.Once);
            act2.Verify(c => c.Invoke(33, 0), Times.Once);
            act2.Verify(c => c.Invoke(42, 1), Times.Once);
            act3.Verify(c => c.Invoke(33), Times.Once);
            act3.Verify(c => c.Invoke(42), Times.Once);

        }

        [Fact]
        public async Task EnumerableAsyncExtensions_SelectAsync_Partitioned_IReadOnlyListInvokesEnumerator()
        {
            var backer = new List<int> { 33, 42 };

            var seq = new Mock<IReadOnlyList<int>>();
            seq.SetupGet(c => c.Count).Returns(backer.Count);
            
            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                 .Returns(() => backer.GetEnumerator());
            
            var (act1, act2, act3) = CreateMockCompletedSelectTaskDelegates<int>();

            await iEnum.Object.SelectAsync(2, act1.Object); // partitioned
            await iEnum.Object.SelectAsync(2, act2.Object);
            await iEnum.Object.SelectAsync(2, act3.Object);

            iEnum.Verify(c => c.GetEnumerator(), Times.Exactly(3)); // once each

            seq.VerifyGet(c => c.Count, Times.Never); // 2 per serial iteration, 1 per unbounded, 0 otherwise
            
            // verify delegate invocations
            act1.Verify(c => c.Invoke(33, 0, default), Times.Once);
            act1.Verify(c => c.Invoke(42, 1, default), Times.Once);
            act2.Verify(c => c.Invoke(33, 0), Times.Once);
            act2.Verify(c => c.Invoke(42, 1), Times.Once);
            act3.Verify(c => c.Invoke(33), Times.Once);
            act3.Verify(c => c.Invoke(42), Times.Once);

        }


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public async Task EnumerableAsyncExtensions_ForEachAsync_NonPartitioned_IListDoesNotInvokeEnumerator(int dop)
        {
            using var cts = new CancellationTokenSource();

            var seq = new Mock<IList<int>>();
            seq.SetupGet(c => c.Count).Returns(1);
            seq.SetupGet(c => c[0]).Returns(33);

            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                .Throws(new Exception("Enumerator should not be called"));

            var (act1, act2, act3) = CreateMockCompletedForEachTaskDelegates();

            await InvokeForEachCallbacksWithDopArg(iEnum.Object, dop, act3.Object, act2.Object, act1.Object, cts.Token);

            iEnum.Verify(c => c.GetEnumerator(), Times.Never);

            // 2 per serial iteration, 1 per unbounded, 0 otherwise
            var cnt = 3 * dop switch
            {
                -1 => 1,
                0 => 1,
                1 => 2,
                _ => 0
            };

            seq.VerifyGet(c => c.Count, Times.Exactly(cnt)); 
            seq.VerifyGet(c => c[0], Times.Exactly(3)); // once each

            // verify delegate invocations
            act1.Verify(c => c.Invoke(33, 0, It.IsAny<CancellationToken>()), Times.Once);
            act2.Verify(c => c.Invoke(33, 0), Times.Once);
            act3.Verify(c => c.Invoke(33), Times.Once);

        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public async Task EnumerableAsyncExtensions_SelectAsync_NonPartitioned_IListDoesNotInvokeEnumerator(int dop)
        {
            using var cts = new CancellationTokenSource();

            var seq = new Mock<IList<int>>();
            seq.SetupGet(c => c.Count).Returns(1);
            seq.SetupGet(c => c[0]).Returns(33);

            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                 .Throws(new Exception("Enumerator should not be called"));

            var (act1, act2, act3) = CreateMockCompletedSelectTaskDelegates<int>();

            await InvokeSelectCallbacksWithDopArg(iEnum.Object, dop, act3.Object, act2.Object, act1.Object, cts.Token);

            iEnum.Verify(c => c.GetEnumerator(), Times.Never);

            // 3 per serial iteration, 1 per unbounded, 0 otherwise
            var cnt = 3 * dop switch
            {
                -1 => 1,
                0 => 1,
                1 => 3,
                _ => 0
            };

            seq.VerifyGet(c => c.Count, Times.Exactly(cnt)); 
            seq.VerifyGet(c => c[0], Times.Exactly(3)); // once each

            // verify delegate invocations
            act1.Verify(c => c.Invoke(33, 0, It.IsAny<CancellationToken>()), Times.Once);
            act2.Verify(c => c.Invoke(33, 0), Times.Once);
            act3.Verify(c => c.Invoke(33), Times.Once);

        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public async Task EnumerableAsyncExtensions_ForEachAsync_NonPartitioned_IReadOnlyListDoesNotInvokeEnumerator(int dop)
        {
            using var cts = new CancellationTokenSource();

            var seq = new Mock<IReadOnlyList<int>>();
            seq.SetupGet(c => c.Count).Returns(1);
            seq.SetupGet(c => c[0]).Returns(33);

            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                .Throws(new Exception("Enumerator should not be called"));

            var (act1, act2, act3) = CreateMockCompletedForEachTaskDelegates();

            await InvokeForEachCallbacksWithDopArg(iEnum.Object, dop, act3.Object, act2.Object, act1.Object, cts.Token);

            iEnum.Verify(c => c.GetEnumerator(), Times.Never);

            // 2 per serial iteration, 1 per unbounded, 0 otherwise
            var cnt = 3 * dop switch
            {
                -1 => 1,
                0 => 1,
                1 => 2,
                _ => 0
            };

            seq.VerifyGet(c => c.Count, Times.Exactly(cnt)); 
            seq.VerifyGet(c => c[0], Times.Exactly(3)); // once each

            // verify delegate invocations
            act1.Verify(c => c.Invoke(33, 0, It.IsAny<CancellationToken>()), Times.Once);
            act2.Verify(c => c.Invoke(33, 0), Times.Once);
            act3.Verify(c => c.Invoke(33), Times.Once);

        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        public async Task EnumerableAsyncExtensions_SelectAsync_NonPartitioned_IReadOnlyListDoesNotInvokeEnumerator(int dop)
        {
            using var cts = new CancellationTokenSource();

            var seq = new Mock<IReadOnlyList<int>>();
            seq.SetupGet(c => c.Count).Returns(1);
            seq.SetupGet(c => c[0]).Returns(33);

            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                 .Throws(new Exception("Enumerator should not be called"));

            var (act1, act2, act3) = CreateMockCompletedSelectTaskDelegates<int>();

            await InvokeSelectCallbacksWithDopArg(iEnum.Object, dop, act3.Object, act2.Object, act1.Object, cts.Token);

            iEnum.Verify(c => c.GetEnumerator(), Times.Never);

            // 3 per serial iteration, 1 per unbounded, 0 otherwise
            var cnt = 3 * dop switch
            {
                -1 => 1,
                0 => 1,
                1 => 3,
                _ => 0
            };

            seq.VerifyGet(c => c.Count, Times.Exactly(cnt)); 
            seq.VerifyGet(c => c[0], Times.Exactly(3)); // once each

            // verify delegate invocations
            act1.Verify(c => c.Invoke(33, 0, It.IsAny<CancellationToken>()), Times.Once);
            act2.Verify(c => c.Invoke(33, 0), Times.Once);
            act3.Verify(c => c.Invoke(33), Times.Once);

        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task EnumerableAsyncExtensions_ForEachAsync_NotIListInvokesEnumerator(int dop)
        {
            using var cts = new CancellationTokenSource();

            var backingList = new List<int> { 42, 33 };

            var iEnum = new Mock<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                .Returns(() => backingList.GetEnumerator());

            var (act1, act2, act3) = CreateMockCompletedForEachTaskDelegates();

            await InvokeForEachCallbacksWithDopArg(iEnum.Object, dop, act3.Object, act2.Object, act1.Object, cts.Token);
            
 
            iEnum.Verify(c => c.GetEnumerator(), Times.Exactly(3)); // once each

            // verify delegate invocations
            act1.Verify(c => c.Invoke(42, 0, It.IsAny<CancellationToken>()), Times.Once);
            act1.Verify(c => c.Invoke(33, 1, It.IsAny<CancellationToken>()), Times.Once);

            act2.Verify(c => c.Invoke(42, 0), Times.Once);
            act2.Verify(c => c.Invoke(33, 1), Times.Once);

            act3.Verify(c => c.Invoke(42), Times.Once);
            act3.Verify(c => c.Invoke(33), Times.Once);

        }


        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task EnumerableAsyncExtensions_SelectAsync_NotIListInvokesEnumerator(int dop)
        {
            using var cts = new CancellationTokenSource();

            var backingList = new List<int> { 42, 33 };

            var iEnum = new Mock<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                 .Returns(() => backingList.GetEnumerator());

            var (act1, act2, act3) = CreateMockCompletedSelectTaskDelegates<int>();

            await InvokeSelectCallbacksWithDopArg(iEnum.Object, dop, act3.Object, act2.Object, act1.Object, cts.Token);
            
 
            iEnum.Verify(c => c.GetEnumerator(), Times.Exactly(3)); // once each

            // verify delegate invocations
            act1.Verify(c => c.Invoke(42, 0, It.IsAny<CancellationToken>()), Times.Once);
            act1.Verify(c => c.Invoke(33, 1, It.IsAny<CancellationToken>()), Times.Once);

            act2.Verify(c => c.Invoke(42, 0), Times.Once);
            act2.Verify(c => c.Invoke(33, 1), Times.Once);

            act3.Verify(c => c.Invoke(42), Times.Once);
            act3.Verify(c => c.Invoke(33), Times.Once);

        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task EnumerableAsyncExtensions_ForEachAsync_SucceedsOnEmptyList(int dop)
        {
            IEnumerable<int> list = new List<int>();

            var (act1, act2, act3) = CreateMockCompletedForEachTaskDelegates();

            await InvokeForEachCallbacksWithDopArg(list, dop, act3.Object, act2.Object, act1.Object);

            act1.Verify(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
            act2.Verify(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>()), Times.Never);
            act3.Verify(c => c.Invoke(It.IsAny<int>()), Times.Never);

        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task EnumerableAsyncExtensions_SelectAsync_SucceedsOnEmptyList(int dop)
        {
            IEnumerable<int> list = new List<int>();

            var (act1, act2, act3) = CreateMockCompletedSelectTaskDelegates<int>();

            await InvokeSelectCallbacksWithDopArg(list, dop, act3.Object, act2.Object, act1.Object);

            act1.Verify(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
            act2.Verify(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>()), Times.Never);
            act3.Verify(c => c.Invoke(It.IsAny<int>()), Times.Never);

        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task EnumerableAsyncExtensions_ForEachAsync_SucceedsOnEmptyEnumerable(int dop)
        {
            var list = new Mock<IEnumerable<int>>();
            var @enum = new Mock<IEnumerator<int>>();

            @enum.Setup(c => c.MoveNext())
                .Returns(false);

            @enum.SetupGet(c => c.Current)
                .Throws(new Exception("Should not get here"));

            list.Setup(c => c.GetEnumerator())
                .Returns(() => @enum.Object);

            var (act1, act2, act3) = CreateMockCompletedForEachTaskDelegates();

            await InvokeForEachCallbacksWithDopArg(list.Object, dop, act3.Object, act2.Object, act1.Object);

            act1.Verify(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
            act2.Verify(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>()), Times.Never);
            act3.Verify(c => c.Invoke(It.IsAny<int>()), Times.Never);
        }
        
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task EnumerableAsyncExtensions_SelectAsync_SucceedsOnEmptyEnumerable(int dop)
        {
            var list = new Mock<IEnumerable<int>>();
            var @enum = new Mock<IEnumerator<int>>();

            @enum.Setup(c => c.MoveNext())
                 .Returns(false);

            @enum.SetupGet(c => c.Current)
                 .Throws(new Exception("Should not get here"));

            list.Setup(c => c.GetEnumerator())
                .Returns(() => @enum.Object);

            var (act1, act2, act3) = CreateMockCompletedSelectTaskDelegates<int>();

            await InvokeSelectCallbacksWithDopArg(list.Object, dop, act3.Object, act2.Object, act1.Object);

            act1.Verify(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
            act2.Verify(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>()), Times.Never);
            act3.Verify(c => c.Invoke(It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public async Task EnumerableAsyncExtensions_ForEachAsync_Serial_RunsInApproximatelySerialTime()
        {
            var wait = new[] { 3, 2, 1, 5, 4 };
            var results = new int[wait.Length];

            var items = Enumerable.Range(0, 5);

            var task = items.ForEachAsync(1, async (s, i, c) =>
            {

                await Task.Delay(wait[i % 5] * 1000, c);

                results[i] = s;

            });

            var start = DateTime.Now;

            await task;

            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, results);

            var duration = DateTime.Now - start;

            _output.WriteLine("Total Duration: {0:g}", duration);

            Assert.True(duration > TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async Task EnumerableAsyncExtensions_SelectEnumerableAsync_Serial_RunsInApproximatelySerialTime()
        {
            var wait = new[] { 3, 2, 1, 5, 4 };

            var items = Enumerable.Range(0, 5);

            var res = items.SelectEnumerableAsync(1, async (s, i, c) =>
            {
                await Task.Delay(wait[i % 5] * 1000, c);
                return s;
            });
            

            var sw = Stopwatch.StartNew();

            var results = new int[5];
            var ix = 0;
            await foreach (var r in res)
            {
                results[ix++] = r;
            }

            sw.Stop();

            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, results);

            _output.WriteLine("Total Duration: {0:g}", sw.Elapsed);

            Assert.True(sw.Elapsed > TimeSpan.FromSeconds(13));
        }


        [Fact]
        public async Task EnumerableAsyncExtensions_SelectAsync_Serial_RunsInApproximatelySerialTime()
        {
            var wait = new[] { 3, 2, 1, 5, 4 };

            var items = Enumerable.Range(0, 5);

            var task = items.SelectAsync(1, async (s, i, c) =>
            {

                await Task.Delay(wait[i % 5] * 1000, c);

                return s;

            });

            var start = DateTime.Now;

            var results = await task;

            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, results);

            var duration = DateTime.Now - start;

            _output.WriteLine("Total Duration: {0:g}", duration);

            Assert.True(duration > TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async Task EnumerableAsyncExtensions_ForEachAsync_Partitioned_RunsInApproximatelyHalfTime()
        {
            var wait = new[] { 3, 2, 1, 2, 1 };
            var results = new int[wait.Length];

            var items = Enumerable.Range(0, 5);

            var task = items.ForEachAsync(2, async (s, i, c) =>
            {

                await Task.Delay(wait[i % 5] * 1000, c);

                results[i] = s;

            });

            var start = DateTime.Now;

            await task;

            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, results);

            var duration = DateTime.Now - start;

            _output.WriteLine("Total Duration: {0:g}", duration);

            Assert.True(duration > TimeSpan.FromSeconds(3));
            Assert.True(duration <= TimeSpan.FromSeconds(8));
        }


        [Fact]
        public async Task EnumerableAsyncExtensions_SelectEnumerableAsync_Partitioned_RunsInApproximatelyHalfTime()
        {
            //var wait = new[] { 3, 2, 1, 2, 1 };
            var wait = new[] { 4, 3, 2, 1, 0 };

            var items = Enumerable.Range(0, 5);

            var res = items.SelectEnumerableAsync(2, async (s, i, c) =>
            {
                await Task.Delay(wait[i % 5] * 1000, c);

                _output.WriteLine("Index: {0}, Item: {1}", i, s);

                return s; //s * (int)i;
            });
            

            var sw = Stopwatch.StartNew();

            var results = new int[5];
            var ix = 0;
            await foreach (var r in res)
            {
                results[ix++] = r;
            }

            sw.Stop();

            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, results); //BUG these shouldn't come out in order 0..5 but should be based on the Delay?

            _output.WriteLine("Total Duration: {0:g}", sw.Elapsed);

            Assert.True(sw.Elapsed > TimeSpan.FromSeconds(3));
            Assert.True(sw.Elapsed <= TimeSpan.FromSeconds(8));
        }


        [Fact]
        public async Task EnumerableAsyncExtensions_SelectAsync_Partitioned_RunsInApproximatelyHalfTime()
        {
            var wait = new[] { 3, 2, 1, 2, 1 };

            var items = Enumerable.Range(0, 5);

            var task = items.SelectAsync(2, async (s, i, c) =>
            {

                await Task.Delay(wait[i % 5] * 1000, c);

                return s * (int)i;

            });

            var start = DateTime.Now;

            var results = await task;

            Assert.Equal(new[] { 0, 1, 4, 9, 16 }, results);

            var duration = DateTime.Now - start;

            _output.WriteLine("Total Duration: {0:g}", duration);

            Assert.True(duration > TimeSpan.FromSeconds(3));
            Assert.True(duration <= TimeSpan.FromSeconds(8));
        }

        [Fact]
        public async Task EnumerableAsyncExtensions_ForEachAsync_Unbounded_RunsAtMaximumBoundedTime()
        {
            var wait = new[] { 3, 2, 1, 2, 1 };
            var results = new int[wait.Length];

            var items = Enumerable.Range(0, 5);

            var task = items.ForEachAsync(0, async (s, i, c) =>
            {

                await Task.Delay(wait[i % 5] * 1000, c);

                results[i] = s;

            });

            var start = DateTime.Now;

            await task;

            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, results);

            var duration = DateTime.Now - start;

            _output.WriteLine("Total Duration: {0:g}", duration);

            Assert.True(duration >= TimeSpan.FromSeconds(2.5));
            Assert.True(duration <= TimeSpan.FromSeconds(5.5));
        }


        [Fact]
        public async Task EnumerableAsyncExtensions_SelectEnumerableAsync_Unbounded_RunsAtMaximumBoundedTime()
        {
            var wait = new[] { 3, 2, 1, 2, 1 };

            var items = Enumerable.Range(0, 5);

            var res = items.SelectEnumerableAsync(2, async (s, i, c) =>
            {
                await Task.Delay(wait[i % 5] * 1000, c);
                return s * 2;
            });
            

            var sw = Stopwatch.StartNew();

            var results = new int[5];
            var ix = 0;
            await foreach (var r in res)
            {
                results[ix++] = r;
            }

            sw.Stop();

            Assert.Equal(new[] { 0, 2, 4, 6, 8 }, results);

            _output.WriteLine("Total Duration: {0:g}", sw.Elapsed);

            Assert.True(sw.Elapsed > TimeSpan.FromSeconds(2.5));
            Assert.True(sw.Elapsed <= TimeSpan.FromSeconds(5.5));
        }

        [Fact]
        public async Task EnumerableAsyncExtensions_SelectAsync_Unbounded_RunsAtMaximumBoundedTime()
        {
            var wait = new[] { 3, 2, 1, 2, 1 };

            var items = Enumerable.Range(0, 5);

            var task = items.SelectAsync(0, async (s, i, c) =>
            {

                await Task.Delay(wait[i % 5] * 1000, c);

                return s * 2;

            });

            var start = DateTime.Now;

            var results = await task;

            Assert.Equal(new[] { 0, 2, 4, 6, 8 }, results);

            var duration = DateTime.Now - start;

            _output.WriteLine("Total Duration: {0:g}", duration);

            Assert.True(duration >= TimeSpan.FromSeconds(2.5));
            Assert.True(duration <= TimeSpan.FromSeconds(5.5));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task EnumerableAsyncExtensions_ForEachAsync_WithScheduler_InvokesOnScheduler(int dop)
        {
            using var cts = new CancellationTokenSource();

            var pair = new ConcurrentExclusiveSchedulerPair();

            var backingList = new List<int> { 42, 33 };

            var seq = new Mock<IList<int>>();
            seq.SetupGet(c => c.Count).Returns(backingList.Count);
            seq.Setup(c => c[It.IsAny<int>()])
                .Returns((int index) => backingList[index]);

            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                .Returns(() => backingList.GetEnumerator());

            var (act1, act2, act3) = CreateMockCompletedForEachTaskDelegates();

            act1.Setup(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                 {
                     var currScheduler = TaskScheduler.Current;
                     Assert.Same(pair.ConcurrentScheduler, currScheduler);
                 });

            act2.Setup(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>()))
                .Callback(() =>
                 {
                     var currScheduler = TaskScheduler.Current;
                     Assert.Same(pair.ConcurrentScheduler, currScheduler);
                 });

            act3.Setup(c => c.Invoke(It.IsAny<int>()))
                .Callback(() =>
                 {
                     var currScheduler = TaskScheduler.Current;
                     Assert.Same(pair.ConcurrentScheduler, currScheduler);
                 });

            await InvokeForEachCallbacksWithDopArg(iEnum.Object, dop, act3.Object, act2.Object, act1.Object, cts.Token, pair.ConcurrentScheduler);

            pair.Complete();

            await pair.Completion;
            

            iEnum.Verify(c => c.GetEnumerator(), dop <= 0 ? Times.Never() : Times.Exactly(3));

            // 0 per serial iteration (on scheduler), 1 per unbounded, 0 otherwise
            var cnt = 3 * dop switch
            {
                -1 => 1,
                0 => 1,
                1 => 0,
                _ => 0
            };

            seq.VerifyGet(c => c.Count, Times.Exactly(cnt)); 

            seq.VerifyGet(c => c[0], dop > 0 ? Times.Never() : Times.Exactly(3)); // once each
            seq.VerifyGet(c => c[1], dop > 0 ? Times.Never() : Times.Exactly(3)); // once each

        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task EnumerableAsyncExtensions_SelectAsync_WithScheduler_InvokesOnScheduler(int dop)
        {
            using var cts = new CancellationTokenSource();

            var pair = new ConcurrentExclusiveSchedulerPair();

            var backingList = new List<int> { 42, 33 };

            var seq = new Mock<IList<int>>();
            seq.SetupGet(c => c.Count).Returns(backingList.Count);
            seq.Setup(c => c[It.IsAny<int>()])
                .Returns((int index) => backingList[index]);

            var iEnum = seq.As<IEnumerable<int>>();
            iEnum.Setup(c => c.GetEnumerator())
                .Returns(() => backingList.GetEnumerator());

            var (act1, act2, act3) = CreateMockCompletedSelectTaskDelegates<int>();

            act1.Setup(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                 {
                     var currScheduler = TaskScheduler.Current;
                     if (dop <= 0)
                     {
                         Assert.Same(pair.ConcurrentScheduler, currScheduler);
                     }
                     else
                     {
                         Assert.NotSame(pair.ConcurrentScheduler, currScheduler);
                         Assert.Same(currScheduler.GetType(), pair.ConcurrentScheduler.GetType());
                         Assert.Equal(dop, currScheduler.MaximumConcurrencyLevel);
                     }
                 });

            act2.Setup(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>()))
                .Callback(() =>
                 {
                     var currScheduler = TaskScheduler.Current;
                     if (dop <= 0)
                     {
                         Assert.Same(pair.ConcurrentScheduler, currScheduler);
                     }
                     else
                     {
                         Assert.NotSame(pair.ConcurrentScheduler, currScheduler);
                         Assert.Same(currScheduler.GetType(), pair.ConcurrentScheduler.GetType());
                         Assert.Equal(dop, currScheduler.MaximumConcurrencyLevel);
                     }
                 });

            act3.Setup(c => c.Invoke(It.IsAny<int>()))
                .Callback(() =>
                 {
                     var currScheduler = TaskScheduler.Current;
                     if (dop <= 0)
                     {
                         Assert.Same(pair.ConcurrentScheduler, currScheduler);
                     }
                     else
                     {
                         Assert.NotSame(pair.ConcurrentScheduler, currScheduler);
                         Assert.Same(currScheduler.GetType(), pair.ConcurrentScheduler.GetType());
                         Assert.Equal(dop, currScheduler.MaximumConcurrencyLevel);
                     }
                 });

            await InvokeSelectCallbacksWithDopArg(iEnum.Object, dop, act3.Object, act2.Object, act1.Object, cts.Token, pair.ConcurrentScheduler);

            pair.Complete();

            await pair.Completion;
            

            iEnum.Verify(c => c.GetEnumerator(), dop <= 0 ? Times.Never() : Times.Exactly(3));

            // 0 per serial iteration (on scheduler), 1 per unbounded, 0 otherwise
            var cnt = 3 * dop switch
            {
                -1 => 1,
                0 => 1,
                1 => 0,
                _ => 0
            };

            seq.VerifyGet(c => c.Count, Times.Exactly(cnt)); 

            seq.VerifyGet(c => c[0], dop > 0 ? Times.Never() : Times.Exactly(3)); // once each
            seq.VerifyGet(c => c[1], dop > 0 ? Times.Never() : Times.Exactly(3)); // once each

        }


        private static (Mock<Func<int, long, CancellationToken, Task<TResult>>> act1, Mock<Func<int, long, Task<TResult>>> act2, Mock<Func<int, Task<TResult>>> act3) CreateMockCompletedSelectTaskDelegates<TResult>()
        {
            var act1 = new Mock<Func<int, long, CancellationToken, Task<TResult>>>();
            act1.Setup(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(default(TResult)));

            var act2 = new Mock<Func<int, long, Task<TResult>>>();
            act2.Setup(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>()))
                .Returns(Task.FromResult(default(TResult)));

            var act3 = new Mock<Func<int, Task<TResult>>>();
            act3.Setup(c => c.Invoke(It.IsAny<int>()))
                .Returns(Task.FromResult(default(TResult)));
            return (act1, act2, act3);
        }

        private static (Mock<Func<int, long, CancellationToken, Task>> act1, Mock<Func<int, long, Task>> act2, Mock<Func<int, Task>> act3) CreateMockCompletedForEachTaskDelegates()
        {
            var act1 = new Mock<Func<int, long, CancellationToken, Task>>();
            act1.Setup(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var act2 = new Mock<Func<int, long, Task>>();
            act2.Setup(c => c.Invoke(It.IsAny<int>(), It.IsAny<long>()))
                .Returns(Task.CompletedTask);

            var act3 = new Mock<Func<int, Task>>();
            act3.Setup(c => c.Invoke(It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            return (act1, act2, act3);
        }


        private static async Task InvokeSelectCallbacksWithDopArg<T, TResult>(
            IEnumerable<T> list, int dop, Func<T, Task<TResult>> act1, Func<T, long, Task<TResult>> act2,
            Func<T, long, CancellationToken, Task<TResult>> act3, CancellationToken token = default, TaskScheduler scheduler = null)
        {

            // ReSharper disable PossibleMultipleEnumeration
            if (dop >= 0)
            {
                await list.SelectAsync(dop, act1, token, scheduler);
                await list.SelectAsync(dop, act2, token, scheduler);
                await list.SelectAsync(dop, act3, token, scheduler);
            }
            else
            {
                await list.SelectAsync(act1, token, scheduler);
                await list.SelectAsync(act2, token, scheduler);
                await list.SelectAsync(act3, token, scheduler);
            }
            // ReSharper restore PossibleMultipleEnumeration

        }

        private static async Task InvokeForEachCallbacksWithDopArg<T>(IEnumerable<T> list, int dop, Func<T, Task> act1, Func<T, long, Task> act2,
            Func<T, long, CancellationToken, Task> act3, CancellationToken token = default, TaskScheduler scheduler = null)
        {

            // ReSharper disable PossibleMultipleEnumeration
            if (dop >= 0)
            {
                await list.ForEachAsync(dop, act1, token, scheduler);
                await list.ForEachAsync(dop, act2, token, scheduler);
                await list.ForEachAsync(dop, act3, token, scheduler);
            }
            else
            {
                await list.ForEachAsync(act1, token, scheduler);
                await list.ForEachAsync(act2, token, scheduler);
                await list.ForEachAsync(act3, token, scheduler);
            }
            // ReSharper restore PossibleMultipleEnumeration

        }
    }
}