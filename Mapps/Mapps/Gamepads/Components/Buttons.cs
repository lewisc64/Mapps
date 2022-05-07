namespace Mapps.Gamepads.Components
{
    public class Buttons<T> : IGamepadComponent
    {
        private bool _disposed;

        public Buttons()
        {
        }

        public IEnumerable<T> HeldButtons { get; internal set; } = new List<T>();

        public bool IsPressed(T button)
        {
            ThrowIfDisposed();
            return HeldButtons.Contains(button);
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
                throw new ObjectDisposedException(nameof(Buttons<T>));
            }
        }
    }
}
