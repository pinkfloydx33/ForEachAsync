using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Floydcom.ForEachAsync.Util;
using Xunit;

namespace Floydcom.ForEachAsync.Tests.Util
{

    public class SynchronizationContextRemoverTests
    {

        [Fact]
        public async Task SynchronizationContextRemover_RemovesContext_WhenAwaitedDirectly()
        {
            await DoSomeWorkAsync(true);

            await new SynchronizationContextRemover();

            Assert.Null(SynchronizationContext.Current);

            await DoSomeWorkAsync(false);

        }

        [Fact]
        public void SynchronizationContextRemover_INotifyCompletionVerification()
        {
            // if not set... set it
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            // store it off
            var original = SynchronizationContext.Current;

            var remover = new SynchronizationContextRemover();

            // should not be completed
            Assert.False(remover.IsCompleted);

            // get awaiter and verify its our Remover, verify equality methods
            var awaiter = remover.GetAwaiter();

            Assert.Equal(remover, awaiter);
            Assert.IsType<SynchronizationContextRemover>(awaiter);
            Assert.True(awaiter.Equals(remover));
            Assert.Equal(0, remover.GetHashCode());
            Assert.False(awaiter.Equals(new object()));

            // does not throw
            remover.GetResult();

            // within the continuation, Current should be null
            ((INotifyCompletion)remover).OnCompleted(() =>
            {
                Assert.Null(SynchronizationContext.Current);
            });

            // but out here... it should be the original value
            Assert.Same(original, SynchronizationContext.Current);
            
        }


        [Fact]
        public async Task SynchronizationContextRemover_RemovesContext_WhenAwaitedViaVariable()
        {
            
            var remover = new SynchronizationContextRemover();

            await DoSomeWorkAsync(true);

            Assert.False(remover.IsCompleted);

            await remover;

            Assert.Null(SynchronizationContext.Current);

            Assert.True(remover.IsCompleted);

            await DoSomeWorkAsync(false);

        }

        private static async Task DoSomeWorkAsync(bool shouldHaveSyncContext)
        {
            
            Assert.Equal(shouldHaveSyncContext, SynchronizationContext.Current != null);

            await Task.Run(() => Thread.Sleep(1000));

            Assert.Equal(shouldHaveSyncContext, SynchronizationContext.Current != null);
        }


        [Fact]
        public async Task SynchronizationContextRemover_DoesNotRemoveContext_IfNotAwaited()
        {
            await DoSomeWorkAsync(true);


#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            // ReSharper disable once UnusedVariable
            var x = new SynchronizationContextRemover();
#pragma warning restore CS0219 // Variable is assigned but its value is never used
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            Assert.NotNull(SynchronizationContext.Current);

            await DoSomeWorkAsync(true);
        }
    }
}
