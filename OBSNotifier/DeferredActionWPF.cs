using System.Windows.Threading;

namespace OBSNotifier
{
    public sealed class DeferredActionWPF : IDisposable
    {
        System.Threading.Timer? close_timer = null;
        DispatcherObject? dsp_object = null;
        readonly Action action;
        readonly int delay = 1000;

        public DeferredActionWPF(Action action, int delay = 1000, DispatcherObject? dispatcherToInvokeOnIt = null)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.delay = delay;
            dsp_object = dispatcherToInvokeOnIt;
        }

        public bool IsTimerActive()
        {
            return close_timer != null;
        }

        public void Cancel()
        {
            close_timer?.Dispose();
            close_timer = null;
        }

        public void CallDeferred()
        {
            close_timer?.Dispose();
            close_timer = new(CallAction, null, delay, Timeout.Infinite);
        }

        void CallAction(object? obj)
        {
            close_timer?.Dispose();
            close_timer = null;
            if (action != null)
            {
                if (dsp_object != null)
                    dsp_object.Dispatcher.Invoke(action);
                else
                    action();
            }
        }

        ~DeferredActionWPF()
        {
            Dispose();
        }

        public void Dispose()
        {
            close_timer?.Dispose();
            close_timer = null;
            dsp_object = null;
        }
    }
}
