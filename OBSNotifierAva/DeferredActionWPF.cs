namespace OBSNotifier
{
    public sealed class DeferredActionWPF : IDisposable
    {
        Timer? close_timer = null;
        bool isUiCall = false;
        readonly Action action;
        readonly int delay = 1000;

        public DeferredActionWPF(Action action, int delay = 1000, bool isUiCall = false)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.delay = delay;
            this.isUiCall = isUiCall;
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
                if (isUiCall)
                    Dispatcher.UIThread.Invoke(action);
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
        }
    }
}
