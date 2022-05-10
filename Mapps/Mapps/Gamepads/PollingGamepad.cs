using System.Diagnostics;

namespace Mapps.Gamepads
{
    public abstract class PollingGamepad : IGamepad
    {
        private bool _disposed;

        private CancellationTokenSource? _cancellationTokenSource = null;

        public bool IsTracking { get; private set; }

        public abstract bool IsConnected { get; protected set; }

        protected abstract TimeSpan PollingInterval { get; }

        protected abstract TimeSpan SendInterval { get; }

        public abstract event EventHandler? OnConnect;

        public abstract event EventHandler? OnDisconnect;

        public abstract event EventHandler? OnStateChanged;

        public virtual void StartTracking()
        {
            ThrowIfDisposed();

            StopTracking();

            _cancellationTokenSource = new CancellationTokenSource();
            new Thread(() => { UpdateLoop(_cancellationTokenSource.Token); }).Start();
            if (SendInterval != TimeSpan.MaxValue)
            {
                new Thread(() => { SendLoop(_cancellationTokenSource.Token); }).Start();
            }

            IsTracking = true;
        }

        public virtual void StopTracking()
        {
            ThrowIfDisposed();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            IsTracking = false;
        }

        protected abstract void UpdateState();

        protected abstract void SendState();

        private void UpdateLoop(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            while (!cancellationToken.IsCancellationRequested)
            {
                UpdateState();
                while (stopwatch.Elapsed < PollingInterval)
                {
                    // do nothing
                }
                stopwatch.Restart();
            }
        }

        private void SendLoop(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            while (!cancellationToken.IsCancellationRequested)
            {
                UpdateState();
                while (stopwatch.Elapsed < PollingInterval)
                {
                    // do nothing
                }
                stopwatch.Restart();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
