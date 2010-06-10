using System;
using System.ComponentModel;
using System.Threading;
using System.Web;

namespace Talifun.Web.LogUrl
{
    public sealed class LogUrlManager : IDisposable
    {
        private readonly TimeSpan lockTimeout = TimeSpan.FromSeconds(10);
        private readonly AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(null);

        private LogUrlManager()
        {
            InitManager();
        }

        public static LogUrlManager Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly LogUrlManager instance = new LogUrlManager();
        }

        /// <summary>
        /// We want to release the manager when app domain is unloaded. So we removed the reference, as nothing will then be referencing
        /// the manager, garbage collector will dispose it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// We are using a sneaky little trick to keep manager alive for the duration of the appdomain.
        /// We are storing a delegate with a reference to the manager in a global area (AppDomain.CurrentDomain.UnhandledException),
        /// which means the garbage collector won't be able to dispose the manager.
        /// HttpModule life is shorter then AppDomain and can be unloaded at any time.
        /// </remarks>
        private void OnDomainUnload(object sender, EventArgs e)
        {
            if (AppDomain.CurrentDomain != null)
            {
                AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
            }
        }

        private void InitManager()
        {
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
        }

        private void DisposeManager()
        {
            beforeLogUrlEvent = null;
            logUrlEvent = null;
            afterLogUrlEvent = null;

            if (AppDomain.CurrentDomain != null)
            {
                AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
            }
        }

        #region LogUrlEvent
        /// <summary>
        /// Where the actual event is stored.
        /// </summary>
        private LogUrlEventHandler logUrlEvent;
        private BeforeLogUrlEventHandler beforeLogUrlEvent;
        private AfterLogUrlEventHandler afterLogUrlEvent;

        /// <summary>
        /// Lock for event delegate access.
        /// </summary>
        private readonly object logUrlEventLock = new object();
        private readonly object beforeLogUrlEventLock = new object();
        private readonly object afterLogUrlEventLock = new object();

        /// <summary>
        /// The event that is fired.
        /// </summary>
        public event LogUrlEventHandler LogUrlEvent
        {
            add
            {
                if (!Monitor.TryEnter(logUrlEventLock, lockTimeout))
                {
                    throw new ApplicationException("Timeout waiting for lock - LogUrlEvent.add");
                }
                try
                {
                    logUrlEvent += value;
                }
                finally
                {
                    Monitor.Exit(logUrlEventLock);
                }
            }
            remove
            {
                if (!Monitor.TryEnter(logUrlEventLock, lockTimeout))
                {
                    throw new ApplicationException("Timeout waiting for lock - LogUrlEvent.remove");
                }
                try
                {
                    logUrlEvent -= value;
                }
                finally
                {
                    Monitor.Exit(logUrlEventLock);
                }
            }
        }

        public event BeforeLogUrlEventHandler BeforeLogUrlEvent
        {
            add
            {
                if (!Monitor.TryEnter(beforeLogUrlEventLock, lockTimeout))
                {
                    throw new ApplicationException("Timeout waiting for lock - BeforeLogUrlEvent.add");
                }
                try
                {
                    beforeLogUrlEvent += value;
                }
                finally
                {
                    Monitor.Exit(beforeLogUrlEventLock);
                }
            }
            remove
            {
                if (!Monitor.TryEnter(beforeLogUrlEventLock, lockTimeout))
                {
                    throw new ApplicationException("Timeout waiting for lock - BeforeLogUrlEvent.remove");
                }
                try
                {
                    beforeLogUrlEvent -= value;
                }
                finally
                {
                    Monitor.Exit(beforeLogUrlEventLock);
                }
            }
        }

        public event AfterLogUrlEventHandler AfterLogUrlEvent
        {
            add
            {
                if (!Monitor.TryEnter(afterLogUrlEventLock, lockTimeout))
                {
                    throw new ApplicationException("Timeout waiting for lock - AfterLogUrlEvent.add");
                }
                try
                {
                    afterLogUrlEvent += value;
                }
                finally
                {
                    Monitor.Exit(afterLogUrlEventLock);
                }
            }
            remove
            {

                if (!Monitor.TryEnter(afterLogUrlEventLock, lockTimeout))
                {
                    throw new ApplicationException("Timeout waiting for lock - AfterLogUrlEvent.remove");
                }
                try
                {
                    afterLogUrlEvent -= value;
                }
                finally
                {
                    Monitor.Exit(afterLogUrlEventLock);
                }
            }
        }

        /// <summary>
        /// Template method to add default behaviour for the event
        /// </summary>
        private void OnLogUrlEvent(LogUrlEventArgs e)
        {
            // TODO: Implement default behaviour of OnLogUrlEvent
        }

        private void OnBeforeLogUrlEvent(BeforeLogUrlEventArgs e)
        {
            // TODO: Implement default behaviour of OnBeforeLogUrlEvent
        }

        private void OnAfterLogUrlEvent(AfterLogUrlEventArgs e)
        {
            // TODO: Implement default behaviour of OnAfterLogUrlEvent
        }

        private void AsynchronousOnLogUrlEventRaised(object state)
        {
            var args = state as LogUrlEventArgs;
            RaiseOnLogUrlEvent(args);
        }

        /// <summary>
        /// Will raise the event on the calling thread synchronously. 
        /// i.e. it will wait until all event handlers have processed the event.
        /// </summary>
        /// <param name="args">The state to be passed to the event.</param>
        private void RaiseCrossThreadOnLogUrlEvent(LogUrlEventArgs args)
        {
            asyncOperation.SynchronizationContext.Send(new SendOrPostCallback(AsynchronousOnLogUrlEventRaised), args);
        }

        /// <summary>
        /// Will raise the event on the calling thread asynchronously. 
        /// i.e. it will immediatly continue processing even though event 
        /// handlers have not processed the event yet.
        /// </summary>
        /// <param name="args">The state to be passed to the event.</param>
        private void RaiseAsynchronousOnLogUrlEvent(LogUrlEventArgs args)
        {
            asyncOperation.Post(new SendOrPostCallback(AsynchronousOnLogUrlEventRaised), args);
        }

        /// <summary>
        /// Will raise the event on the current thread synchronously.
        /// i.e. it will wait until all event handlers have processed the event.
        /// </summary>
        /// <param name="args">The state to be passed to the event.</param>
        private void RaiseOnLogUrlEvent(LogUrlEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.

            LogUrlEventHandler eventHandler;

            if (!Monitor.TryEnter(logUrlEventLock, lockTimeout))
            {
                throw new ApplicationException("Timeout waiting for lock - RaiseOnLogUrlEvent");
            }
            try
            {
                eventHandler = logUrlEvent;
            }
            finally
            {
                Monitor.Exit(logUrlEventLock);
            }

            OnLogUrlEvent(args);

            if (eventHandler != null)
            {
                eventHandler(this, args);
            }
        }

        private void AsynchronousOnBeforeLogUrlEventRaised(object state)
        {
            var args = state as BeforeLogUrlEventArgs;
            RaiseOnBeforeLogUrlEvent(args);
        }

        /// <summary>
        /// Will raise the event on the calling thread synchronously. 
        /// i.e. it will wait until all event handlers have processed the event.
        /// </summary>
        /// <param name="args">The state to be passed to the event.</param>
        private void RaiseCrossThreadOnBeforeLogUrlEvent(BeforeLogUrlEventArgs args)
        {
            asyncOperation.SynchronizationContext.Send(new SendOrPostCallback(AsynchronousOnBeforeLogUrlEventRaised), args);
        }

        /*
        // Do not call this method asynchronously as you will not be able to return feedback via cancel argument
        /// <summary>
        /// Will raise the event on the calling thread asynchronously. 
        /// i.e. it will immediatly continue processing even though event 
        /// handlers have not processed the event yet.
        /// </summary>
        /// <param name="state">The state to be passed to the event.</param>
        private void RaiseAsynchronousOnBeforeLogUrlEvent(BeforeLogUrlEventArgs args)
        {
            asyncOperation.Post(new SendOrPostCallback(AsynchronousOnBeforeLogUrlEventRaised), args);
        }
        */

        /// <summary>
        /// Will raise the event on the current thread synchronously.
        /// i.e. it will wait until all event handlers have processed the event.
        /// </summary>
        /// <param name="args">The state to be passed to the event.</param>
        private void RaiseOnBeforeLogUrlEvent(BeforeLogUrlEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.

            BeforeLogUrlEventHandler eventHandler;

            if (!Monitor.TryEnter(beforeLogUrlEventLock, lockTimeout))
            {
                throw new ApplicationException("Timeout waiting for lock - RaiseOnBeforeLogUrlEvent");
            }
            try
            {
                eventHandler = beforeLogUrlEvent;
            }
            finally
            {
                Monitor.Exit(beforeLogUrlEventLock);
            }

            OnBeforeLogUrlEvent(args);

            if (eventHandler != null)
            {
                eventHandler(this, args);
            }
        }

        private void AsynchronousOnAfterLogUrlEventRaised(object state)
        {
            var args = state as AfterLogUrlEventArgs;
            RaiseOnAfterLogUrlEvent(args);
        }

        /// <summary>
        /// Will raise the event on the calling thread synchronously. 
        /// i.e. it will wait until all event handlers have processed the event.
        /// </summary>
        /// <param name="args">The state to be passed to the event.</param>
        private void RaiseCrossThreadOnAfterLogUrlEvent(AfterLogUrlEventArgs args)
        {
            asyncOperation.SynchronizationContext.Send(new SendOrPostCallback(AsynchronousOnAfterLogUrlEventRaised), args);
        }

        /// <summary>
        /// Will raise the event on the calling thread asynchronously. 
        /// i.e. it will immediatly continue processing even though event 
        /// handlers have not processed the event yet.
        /// </summary>
        /// <param name="args">The state to be passed to the event.</param>
        private void RaiseAsynchronousOnAfterLogUrlEvent(AfterLogUrlEventArgs args)
        {
            asyncOperation.Post(new SendOrPostCallback(AsynchronousOnAfterLogUrlEventRaised), args);
        }

        /// <summary>
        /// Will raise the event on the current thread synchronously.
        /// i.e. it will wait until all event handlers have processed the event.
        /// </summary>
        /// <param name="args">The state to be passed to the event.</param>
        private void RaiseOnAfterLogUrlEvent(AfterLogUrlEventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.

            AfterLogUrlEventHandler eventHandler;

            if (!Monitor.TryEnter(afterLogUrlEventLock, lockTimeout))
            {
                throw new ApplicationException("Timeout waiting for lock - RaiseOnAfterLogUrlEvent");
            }
            try
            {
                eventHandler = afterLogUrlEvent;
            }
            finally
            {
                Monitor.Exit(afterLogUrlEventLock);
            }

            OnAfterLogUrlEvent(args);

            if (eventHandler != null)
            {
                eventHandler(this, args);
            }
        }
        #endregion

        #region IDisposable Members
        private int alreadyDisposed = 0;

        ~LogUrlManager()
        {
            // call Dispose with false.  Since we're in the
            // destructor call, the managed resources will be
            // disposed of anyways.
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (alreadyDisposed != 0) return;

            // dispose of the managed and unmanaged resources
            Dispose(true);

            // tell the GC that the Finalize process no longer needs
            // to be run for this object. 

            // it is called after Dispose(true) to ensure that GC.SuppressFinalize() 
            // only gets called if the Dispose operation completes successfully. 
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            var disposedAlready = Interlocked.Exchange(ref alreadyDisposed, 1);
            if (disposedAlready != 0) return;

            if (!disposeManagedResources) return;

            // Dispose managed resources.
            DisposeManager();
        }

        #endregion

        /// <summary>
        /// Raise log url event
        /// </summary>
        /// <param name="httpApplication">The httpApplication the request was raised on.</param>
        /// <param name="expression">The expression that matched the url.</param>
        internal void RaiseLogUrlEvent(HttpApplication httpApplication, string expression)
        {
            var beforeLogUrlEventArgs = new BeforeLogUrlEventArgs(httpApplication, expression);
            RaiseOnBeforeLogUrlEvent(beforeLogUrlEventArgs);

            if (beforeLogUrlEventArgs.Cancel) return;

            var logUrlEventArgs = new LogUrlEventArgs(httpApplication, expression);
            RaiseOnLogUrlEvent(logUrlEventArgs);

            var afterLogUrlEventArgs = new AfterLogUrlEventArgs(httpApplication, expression);
            RaiseOnAfterLogUrlEvent(afterLogUrlEventArgs);
        }
    }
}
