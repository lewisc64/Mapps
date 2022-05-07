namespace Mapps.Gamepads.Components
{
    public class Joystick : IGamepadComponent
    {
        private bool _disposed;

        private float _x;

        private float _y;

        public Joystick()
        {
        }

        public float X
        {
            get
            {
                ThrowIfDisposed();
                return ApplyDeadZone(_x);
            }

            internal set
            {
                ThrowIfDisposed();
                _x = value;
            }
        }

        public float Y
        {
            get
            {
                ThrowIfDisposed();
                return ApplyDeadZone(_y);
            }

            internal set
            {
                ThrowIfDisposed();
                _y = value;
            }
        }

        public float DeadZone { get; set; } = 0.1f;

        private float ApplyDeadZone(float value)
        {
            ThrowIfDisposed();

            return Math.Pow(_x, 2) + Math.Pow(_y, 2) < Math.Pow(DeadZone, 2) ? 0 : value;
        }

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
                throw new ObjectDisposedException(nameof(Joystick));
            }
        }
    }
}
