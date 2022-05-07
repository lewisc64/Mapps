namespace Mapps.Gamepads.Components
{
    public class Trigger : IGamepadComponent
    {
        private bool _disposed;

        private float _pressure;

        public Trigger()
        {
        }

        public float Pressure
        {
            get
            {
                ThrowIfDisposed();
                return _pressure < DeadZone ? 0 : _pressure;
            }

            internal set
            {
                ThrowIfDisposed();
                _pressure = value;
            }
        }

        public float DeadZone { get; set; } = 0.0f;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // nothing to dispose
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Trigger));
            }
        }
    }
}
