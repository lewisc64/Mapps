namespace Mapps.Gamepads.Components
{
    public class MutableRGBLight : IGamepadComponent
    {
        private bool _disposed;

        public MutableRGBLight()
        {
        }

        public byte Red { get; set; } = 255;

        public byte Green { get; set; } = 255;

        public byte Blue { get; set; } = 255;

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
