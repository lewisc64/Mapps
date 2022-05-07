using HidLibrary;
using Mapps.Gamepads.Components;
using System.Diagnostics;

namespace Mapps.Gamepads.DualShock4
{
    public class DualShock4 : IGamepad
    {
        public const int VendorId = 0x054C;

        public static readonly int[] ProductIds = new[] { 0x05C4, 0x09CC };

        private HidFastReadDevice? _hidDevice;

        private TimeSpan _outputReportInterval = TimeSpan.FromMilliseconds(0);

        private CancellationTokenSource? _cancellationTokenSource = null;

        private bool _disposed;

        public event EventHandler<EventArgs>? StateChanged;

        public DualShock4()
        {
            LightBar.Red = 0;
            LightBar.Green = 255;
            LightBar.Blue = 255;
        }

        public string DevicePath => _hidDevice?.DevicePath ?? throw new InvalidOperationException("Not connected to a device.");

        public bool IsBluetooth => _hidDevice?.Capabilities.InputReportByteLength > 64;

        public bool IsConnected { get; private set; }

        public Battery Battery { get; } = new Battery();

        public Buttons<DS4Button> Buttons { get; } = new Buttons<DS4Button>();

        public Joystick LeftJoystick { get; } = new Joystick();

        public Joystick RightJoystick { get; } = new Joystick();

        public Trigger LeftTrigger { get; } = new Trigger();

        public Trigger RightTrigger { get; } = new Trigger();

        public MutableRGBLight LightBar { get; } = new MutableRGBLight();

        public RumbleMotor LeftHeavyMotor { get; } = new RumbleMotor();

        public RumbleMotor RightLightMotor { get; } = new RumbleMotor();

        public void Connect(string devicePath)
        {
            ThrowIfDisposed();

            _hidDevice = (HidFastReadDevice)new HidFastReadEnumerator().GetDevice(devicePath);

            _cancellationTokenSource = new CancellationTokenSource();
            new Thread(() => { ProcessHidReports(_cancellationTokenSource.Token); }).Start();
            new Thread(() => { SendHidReports(_cancellationTokenSource.Token); }).Start();
            IsConnected = true;
        }

        public void Disconnect()
        {
            ThrowIfDisposed();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            IsConnected = false;
        }

        public void TestRumble()
        {
            ThrowIfDisposed();

            try
            {
                LeftHeavyMotor.Intensity = 1;
                RightLightMotor.Intensity = 1;
                Thread.Sleep(200);
            }
            finally
            {
                LeftHeavyMotor.Intensity = 0;
                RightLightMotor.Intensity = 0;
            }
        }

        private void ProcessHidReports(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            var stopwatch = Stopwatch.StartNew();

            while (!cancellationToken.IsCancellationRequested && _hidDevice != null)
            {
                if (!_hidDevice.IsConnected)
                {
                    Console.WriteLine("Device disconnected.");
                    Disconnect();
                    return;
                }

                var report = GetNextReport();
                UpdateStateFromPayload(new HidReportPayload(report, IsBluetooth));

                Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Restart();
            }
        }

        private void SendHidReports(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            const int reportLength = 334;
            var report = new HidOutputReport();

            while (!cancellationToken.IsCancellationRequested && _hidDevice != null)
            {
                report.LightBarRed = LightBar.Red;
                report.LightBarGreen = LightBar.Green;
                report.LightBarBlue = LightBar.Blue;

                report.LeftHeavyMotor = (byte)(LeftHeavyMotor.Intensity * 255);
                report.RightLightMotor = (byte)(RightLightMotor.Intensity * 255);

                if (IsBluetooth)
                {
                    SendReport(report.AsBytesBluetooth(reportLength, _outputReportInterval.Milliseconds));
                }
                else
                {
                    SendReport(report.AsBytesUSB(reportLength));
                }

                if (_outputReportInterval.TotalMilliseconds > 0)
                {
                    Thread.Sleep(_outputReportInterval);
                }
            }
        }

        private float ConvertJoystickValue(byte value)
        {
            return value / 255f * 2 - 1;
        }

        private byte[] GetNextReport()
        {
            if (_hidDevice == null)
            {
                return new byte[0];
            }
            return _hidDevice.FastReadReport().Data;
        }

        private void SendReport(byte[] data)
        {
            _hidDevice?.WriteReport(new HidReport(data.Length, new HidDeviceData(data, HidDeviceData.ReadStatus.Success)));
        }

        private void UpdateStateFromPayload(HidReportPayload payload)
        {
            Battery.Percentage = payload.BatteryPercentage;

            Buttons.HeldButtons = payload.HeldButtons;

            LeftJoystick.SetPosition(ConvertJoystickValue(payload.LeftJoystickX), -ConvertJoystickValue(payload.LeftJoystickY));
            RightJoystick.SetPosition(ConvertJoystickValue(payload.RightJoystickX), -ConvertJoystickValue(payload.RightJoystickY));

            LeftTrigger.SetPressure(payload.LeftTriggerPressure / 255f);
            RightTrigger.SetPressure(payload.RightTriggerPressure / 255f);

            StateChanged?.Invoke(this, new EventArgs());
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

                    _hidDevice?.Dispose();

                    StateChanged = null;
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
