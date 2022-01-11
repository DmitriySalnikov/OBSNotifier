using System;
using System.Threading;
using System.Windows.Threading;

namespace OBSNotifier
{
    public class DeferredAction : IDisposable
    {
        Timer close_timer = null;
        DispatcherObject dsp_object = null;
        Action action = null;
        int delay = 1000;

        public DeferredAction(Action action, int delay = 1000, DispatcherObject dispatcherToInvokeOnIt = null)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            this.action = action;
            this.delay = delay;
            dsp_object = dispatcherToInvokeOnIt;
        }

        public void Cancel()
        {
            close_timer?.Dispose();
            close_timer = null;
        }

        public void CallDeferred()
        {
            close_timer?.Dispose();
            close_timer = new Timer(CallAction, null, delay, Timeout.Infinite);
        }

        void CallAction(object obj)
        {
            if (action != null)
            {
                if (dsp_object != null)
                    dsp_object.Dispatcher.Invoke(action);
                else
                    action();
            }
        }

        ~DeferredAction()
        {
            Dispose();
        }

        public void Dispose()
        {
            close_timer?.Dispose();
            close_timer = null;
            action = null;
            dsp_object = null;
        }
    }
}
