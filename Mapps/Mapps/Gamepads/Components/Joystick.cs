namespace Mapps.Gamepads.Components
{
    public class Joystick : IGamepadComponent, IDisposable
    {
        private bool _disposed;

        public Joystick()
        {
        }

        public byte X { get; internal set; }

        public byte Y { get; internal set; }

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
    }
}
