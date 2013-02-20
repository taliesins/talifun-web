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
            var list = enumerable.ToArray();
            var count = list.Count();

            if (count == 0)
            {
                return;
            }

            if (count == 1)
            {
                // if there's only one element, just execute it
                action(list.First());

                return;
            }

            var countdownEvents = new CountdownEvent(count);

            foreach (var item in list)
            {
                ThreadPool.QueueUserWorkItem((object data) =>
                    {
                        var manualResetEvent = (CountdownEvent)((object[])data)[0];
                        var actionToExecute = (Action<T>)((object[])data)[1];
                        var itemForAction = (T)((object[])data)[2];

                        // Execute the method and pass in the enumerated item
                        actionToExecute(itemForAction);

                        // Tell the calling thread that we're done
                        manualResetEvent.Signal();
                    }, new object[] { countdownEvents, action, item });
            }

            countdownEvents.Wait();
        }
    }
}
