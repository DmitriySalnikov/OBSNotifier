using System;
using System.Threading;

namespace OBSNotifier
{
    public class DeferredAction : IDisposable
    {
        Timer close_timer = null;
        Action action = null;
        int delay = 1000;

        public DeferredAction(Action action, int delay = 1000)
        {
            this.action = action;
            this.delay = delay;
        }

        public void Cancel()
        {
            close_timer?.Dispose();
            close_timer = null;
        }

        public void CallDeferred()
        {
            close_timer?.Dispose();
            close_timer = new Timer((ev) =>
            {
                action();
            }, null, delay, Timeout.Infinite);
        }

        ~DeferredAction()
        {
            Dispose();
        }

        public void Dispose()
        {
            close_timer?.Dispose();
            close_timer = null;
        }
    }
}
