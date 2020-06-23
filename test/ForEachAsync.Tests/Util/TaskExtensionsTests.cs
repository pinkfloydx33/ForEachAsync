using System;
using System.Threading;
using System.Threading.Tasks;
using Floydcom.ForEachAsync.Util;
using Xunit;
using TaskExtensions = Floydcom.ForEachAsync.Util.TaskExtensions;
// ReSharper disable AssignNullToNotNullAttribute

namespace Floydcom.ForEachAsync.Tests.Util
{
    public class TaskExtensionsTests
    {
        [Fact]
        public async Task TaskExtensions_WaitAsync_ThrowsOnNullTask()
        {
            Task nullTask = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullTask.WaitAsync(default));
        }

        [Fact]
        public async Task TaskExtensions_WaitAsync_DefaultToken_ReturnsOriginalTask()
        {
            using var t = Task.Run(() => { });
            var res = t.WaitAsync(default);
            Assert.Same(t, res);

            await t;
        }

        [Fact]
        public async Task TaskExtensions_WaitAsync_CancelledToken_ReturnsCancelledTask()
        {
            using var t = Task.Run(() => { });
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var res = t.WaitAsync(cts.Token);
            Assert.NotSame(t, res);
            Assert.True(res.IsCanceled);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => res);

            // discard
            await t;
        }

        [Fact]
        public void TaskExtensions_CancellationTokenTaskSource_FromDefaultToken_IsNotCancelled()
        {
            using var ctts = new TaskExtensions.CancellationTokenTaskSource<object>(CancellationToken.None);
            var task = ctts.Task;
            Assert.NotNull(task);
            Assert.False(task.IsCanceled);

            
        }

        [Fact]
        public void TaskExtensions_CancellationTokenTaskSource_FromCancelledToken_IsCancelled()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            using var ctts = new TaskExtensions.CancellationTokenTaskSource<object>(cts.Token);
            using var task = ctts.Task;
            Assert.NotNull(task);
            Assert.True(task.IsCanceled);

        }

        [Fact]
        public async Task TaskExtensions_CancellationTokenTaskSource_FromUnCancelledToken_GetsCancelled()
        {
            using var cts = new CancellationTokenSource();
            using var ctts = new TaskExtensions.CancellationTokenTaskSource<object>(cts.Token);
            using var task = ctts.Task;
            Assert.NotNull(task);
            Assert.False(task.IsCanceled);

            cts.CancelAfter(1000);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);

            Assert.True(task.IsCanceled);
        }

        [Fact]
        public async Task TaskExtensions_WaitAsyncT_ThrowsOnNullTask()
        {
            Task<int> nullTask = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => nullTask.WaitAsync(default));
        }

        [Fact]
        public async Task TaskExtensions_WaitAsyncT_DefaultToken_ReturnsOriginalTask()
        {
            using var t = Task.Run(() => 3);
            var res = t.WaitAsync(default);
            Assert.Same(t, res);

            var resT = await res;

            Assert.Equal(3, resT);
        }

        [Fact]
        public async Task TaskExtensions_WaitAsyncT_CancelledToken_ReturnsCancelledTask()
        {
            using var t = Task.Run(() => 3);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var res = t.WaitAsync(cts.Token);
            Assert.NotSame(t, res);
            Assert.True(res.IsCanceled);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => res);

            // discard
            await t;

        }

    }
}
