namespace Mapps.Gamepads.Components
{
    public class RumbleMotor : IGamepadComponent
    {
        private bool _disposed;

        public RumbleMotor()
        {
        }

        public float Intensity { get; set; } = 0.0f;

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
