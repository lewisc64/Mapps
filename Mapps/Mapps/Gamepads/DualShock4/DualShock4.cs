using HidLibrary;
using Mapps.Gamepads.Components;

namespace Mapps.Gamepads.DualShock4
{
    public class DualShock4 : IGamepad, IDisposable
    {
        public const int VendorId = 0x054C;

        public static readonly int[] DeviceIds = new[] { 0x05C4, 0x09CC };

        private HidDevice _hidDevice;

        private CancellationTokenSource? _cancellationTokenSource = null;

        private bool _disposed;

        public DualShock4(HidDevice hidDevice)
        {
            _hidDevice = hidDevice;
        }

        public bool Running { get; private set; }

        public Battery Battery { get; } = new Battery();

        public Buttons<DS4Button> Buttons { get; } = new Buttons<DS4Button>();

        public Joystick LeftJoystick { get; } = new Joystick();

        public Joystick RightJoystick { get; } = new Joystick();

        public Trigger LeftTrigger { get; } = new Trigger();

        public Trigger RightTrigger { get; } = new Trigger();

        public void StartTracking()
        {
            ThrowIfDisposed();
            _cancellationTokenSource = new CancellationTokenSource();
            new Thread(() => { ProcessHidReports(_cancellationTokenSource.Token); }).Start();
            Running = true;
        }

        public void StopTracking()
        {
            ThrowIfDisposed();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            Running = false;
        }

        private void ProcessHidReports(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            while (!cancellationToken.IsCancellationRequested)
            {
                var rawReport = _hidDevice.ReadReport();
                var payload = new HidReportPayload(rawReport.Data);

                Battery.Percentage = payload.BatteryPercentage;

                Buttons.HeldButtons = payload.HeldButtons;

                LeftJoystick.X = payload.LeftJoystickX;
                LeftJoystick.Y = payload.LeftJoystickY;
                RightJoystick.X = payload.RightJoystickX;
                RightJoystick.Y = payload.RightJoystickY;

                LeftTrigger.Pressure = payload.LeftTriggerPressure;
                RightTrigger.Pressure = payload.RightTriggerPressure;
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

                    Buttons.Dispose();
                    LeftJoystick.Dispose();
                    RightJoystick.Dispose();
                    LeftTrigger.Dispose();
                    RightTrigger.Dispose();
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
                throw new ObjectDisposedException(nameof(DualShock4));
            }
        }
    }
}
