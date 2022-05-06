namespace Mapps.Gamepads.Components
{
    public class Trigger : IGamepadComponent, IDisposable
    {
        private bool _disposed;

        public Trigger()
        {
        }

        public byte Pressure { get; internal set; }

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
