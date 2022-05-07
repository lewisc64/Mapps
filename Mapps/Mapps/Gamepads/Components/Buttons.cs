namespace Mapps.Gamepads.Components
{
    public class Buttons<T> : IGamepadComponent
    {
        private bool _disposed;

        private IEnumerable<T> _heldButtons = new List<T>();

        public Buttons()
        {
        }

        public event EventHandler<T>? ButtonDown;

        public event EventHandler<T>? ButtonUp;

        public IEnumerable<T> HeldButtons
        {
            get
            {
                ThrowIfDisposed();
                return _heldButtons;
            }

            internal set
            {
                ThrowIfDisposed();

                foreach (var button in _heldButtons.Except(value))
                {
                    Task.Run(() =>
                    {
                        ButtonUp?.Invoke(this, button);
                    });
                }

                foreach (var button in value.Except(_heldButtons))
                {
                    Task.Run(() =>
                    {
                        ButtonDown?.Invoke(this, button);
                    });
                }

                _heldButtons = value;
            }
        }

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
