namespace Mapps.Gamepads.Components
{
    public class Battery : IGamepadComponent, IDisposable
    {
        private bool _disposed;

        public Battery()
        {
        }

        public double Percentage { get; internal set; }

        public bool Charging { get; internal set; }

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
