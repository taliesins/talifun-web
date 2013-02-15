using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Talifun.Web.Helper
{
    public class ParallelExecute
    {
        public static void EachParallel<T>(IEnumerable<T> enumerable, Action<T> action)
        {
            // enumerate the list so it can't change during execution
            // TODO: why is this happening?
            var list = enumerable.ToArray();
            var count = list.Count();

            if (count == 0)
            {
                return;
            }
            else if (count == 1)
            {
                // if there's only one element, just execute it
                action(list.First());
            }
            else
            {
                // Launch each method in it's own thread
                const int maxHandles = 64;
                for (var offset = 0; offset <= count / maxHandles; offset++)
                {
                    // break up the list into 64-item chunks because of a limitiation in WaitHandle
                    var chunk = list.Skip(offset * maxHandles).Take(maxHandles).ToArray();

                    // Initialize the reset events to keep track of completed threads
                    var resetEvents = new WaitHandle[chunk.Count()];

                    // spawn a thread for each item in the chunk
                    var i = 0;
                    foreach (var item in chunk)
                    {
                        resetEvents[i] = new ManualResetEvent(false);
                        ThreadPool.QueueUserWorkItem((object data) =>
                        {
                            var manualResetEvent = (ManualResetEvent)((object[])data)[0];
                            var actionToExecute = (Action<T>)((object[])data)[1];
                            var itemForAction = (T)((object[])data)[2];

                            // Execute the method and pass in the enumerated item
                            actionToExecute(itemForAction);

                            // Tell the calling thread that we're done
                            manualResetEvent.Set();
                        }, new object[] { resetEvents[i], action, item });
                        i++;
                    }

                    // Wait for all threads to execute
                    WaitHandle.WaitAll(resetEvents);
                }
            }
        }
    }
}
