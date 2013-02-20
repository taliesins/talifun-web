#if NET35
using System;
using System.Threading;

namespace Talifun.Web.Helper
{
    /// <summary>
    /// Represents a synchronization primitive that is signaled when its count reaches zero.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   This class is similar to but less versatile than .Net 4's built-in CountdownEvent.
    /// </para>
    /// </remarks>
    public sealed class CountdownEvent : IDisposable
    {
        private readonly ManualResetEvent _reachedZeroEvent = new ManualResetEvent(false);
        private volatile int _count;
        private volatile bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountdownEvent"/> class.
        /// </summary>
        /// <param name="initialCount">The initial count.</param>
        public CountdownEvent(int initialCount)
        {
            _count = initialCount;
        }

        // Disable volatile not treated as volatile warning.
#pragma warning disable 420

        /// <summary>
        /// Signals the event by decrementing the count by one.
        /// </summary>
        /// <returns><see langword="true" /> if the count reached zero and the event was signalled; otherwise, <see langword="false"/>.</returns>
        public bool Signal()
        {
            CheckDisposed();

            // This is not meant to prevent _count from dropping below zero (that can still happen due to race conditions),
            // it's just a simple way to prevent the function from doing unnecessary work if the count has already reached zero.
            if (_count <= 0)
                return true;

            if (Interlocked.Decrement(ref _count) <= 0)
            {
                _reachedZeroEvent.Set();
                return true;
            }
            return false;
        }

#pragma warning restore 420

        /// <summary>
        /// Blocks the calling thread until the <see cref="CountdownEvent"/> is set.
        /// </summary>
        public void Wait()
        {
            CheckDisposed();
            _reachedZeroEvent.WaitOne();
        }

        /// <summary>
        /// Blocks the calling thread until the <see cref="CountdownEvent"/> is set, using a <see cref="TimeSpan"/> to measure the timeout.
        /// </summary>
        /// <param name="timeout">The timeout to wait, or a <see cref="TimeSpan"/> representing -1 milliseconds to wait indefinitely.</param>
        /// <returns><see langword="true"/> if the <see cref="CountdownEvent"/> was set; otherwise, <see langword="false"/>.</returns>
        public bool Wait(TimeSpan timeout)
        {
            CheckDisposed();
            return _reachedZeroEvent.WaitOne(timeout, false);
        }

        /// <summary>
        /// Blocks the calling thread until the <see cref="CountdownEvent"/> is set, using a 32-bit signed integer to measure the timeout.
        /// </summary>
        /// <param name="millisecondsTimeout">The timeout to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
        /// <returns><see langword="true"/> if the <see cref="CountdownEvent"/> was set; otherwise, <see langword="false"/>.</returns>
        public bool Wait(int millisecondsTimeout)
        {
            CheckDisposed();
            return _reachedZeroEvent.WaitOne(millisecondsTimeout, false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    ((IDisposable)_reachedZeroEvent).Dispose();
                _disposed = true;
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(typeof(CountdownEvent).FullName);
        }
    }
}
#endif