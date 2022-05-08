using HidSharp;
using Mapps.Gamepads.Components;
using Mapps.Trackers;
using System.Diagnostics;

namespace Mapps.Gamepads.DualShock4
{
    public class DualShock4 : IGamepad, IHasButtons<DS4Button>, IHasDualJoysticks, IHasDualTriggers, IHasTwoDistinctMassRumbleMotors, IHasBattery
    {
        private const int VendorId = 0x054C;

        private static readonly int[] ProductIds = new[] { 0x05C4, 0x09CC };

        private readonly string _serialNumber;

        private HidDevice? _hidDevice;

        private HidStream? _hidStream;

        private TimeSpan _outputReportInterval = TimeSpan.FromMilliseconds(0);

        private CancellationTokenSource? _cancellationTokenSource = null;

        private CancellationTokenSource? _hidCancellationTokenSource = null;

        private bool _disposed;

        public event EventHandler? OnConnect;

        public event EventHandler? OnDisconnect;

        public event EventHandler? OnStateChanged;

        public DualShock4(string serialNumber)
        {
            _serialNumber = serialNumber;
        }

        public bool IsBluetooth => _hidDevice?.GetMaxInputReportLength() > 64;

        public bool IsTracking { get; private set; }

        public bool IsConnected { get; private set; }

        public NumberTracker MeasuredPollingRate { get; } = new NumberTracker(500);

        public Battery Battery { get; } = new Battery();

        public Buttons<DS4Button> Buttons { get; } = new Buttons<DS4Button>();

        public Joystick LeftJoystick { get; } = new Joystick();

        public Joystick RightJoystick { get; } = new Joystick();

        public Trigger LeftTrigger { get; } = new Trigger();

        public Trigger RightTrigger { get; } = new Trigger();

        public RumbleMotor HeavyMotor { get; } = new RumbleMotor();

        public RumbleMotor LightMotor { get; } = new RumbleMotor();

        public MutableRGBLight LightBar { get; } = new MutableRGBLight(0, 255, 0);

        private static string GetSerialNumber(HidDevice device)
        {
            if (device.GetMaxInputReportLength() == 64)
            {
                var buffer = new byte[64];
                buffer[0] = 18;

                using (var stream = device.Open())
                {
                    stream.GetFeature(buffer);
                }

                return Convert.ToHexString(buffer.Skip(1).Take(6).Reverse().ToArray()).ToLower();
            }
            else
            {
                // HidDevice.GetSerialNumber does not work for wired connections.
                return device.GetSerialNumber().ToLower();
            }
        }

        public static IEnumerable<string> GetSerialNumbers()
        {
            var serialNumbers = new List<string>();

            var devices = DeviceList.Local.GetHidDevices()
                .Where(x => x.VendorID == VendorId && ProductIds.Contains(x.ProductID));

            foreach (var device in devices)
            {
                serialNumbers.Add(GetSerialNumber(device));
            }

            return serialNumbers.Distinct();
        }

        public void StartTracking()
        {
            ThrowIfDisposed();

            StopTracking();

            _cancellationTokenSource = new CancellationTokenSource();
            new Thread(() => { ManageDevices(_cancellationTokenSource.Token); }).Start();

            IsTracking = true;
        }

        public void StopTracking()
        {
            ThrowIfDisposed();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            IsTracking = false;
        }

        public void TestRumble()
        {
            ThrowIfDisposed();

            try
            {
                HeavyMotor.Intensity = 1;
                LightMotor.Intensity = 1;
                Thread.Sleep(200);
            }
            finally
            {
                HeavyMotor.Intensity = 0;
                LightMotor.Intensity = 0;
            }
        }

        private void ManageDevices(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // prefer wired connection
                    var desiredDevice = GetRelevantDevices().OrderBy(x => x.GetMaxInputReportLength()).FirstOrDefault();

                    if (desiredDevice != null && (_hidDevice == null || _hidDevice.DevicePath != desiredDevice.DevicePath))
                    {
                        if (IsConnected)
                        {
                            DisconnectDevice();
                        }
                        ConnectDevice(desiredDevice.DevicePath);
                    }

                    if (_hidDevice != null && !IsConnected)
                    {
                        IsConnected = true;
                        OnConnect?.Invoke(this, EventArgs.Empty);
                    }

                    if (_hidDevice == null && IsConnected)
                    {
                        IsConnected = false;
                        OnDisconnect?.Invoke(this, EventArgs.Empty);
                    }

                    Thread.Sleep(500);
                }
            }
            finally
            {
                if (!_disposed)
                {
                    DisconnectDevice();
                }
            }
        }

        private void ConnectDevice(string devicePath)
        {
            ThrowIfDisposed();

            _hidDevice = DeviceList.Local.GetHidDevices()
                .First(x => x.DevicePath == devicePath);

            _hidStream = _hidDevice.Open();

            _hidCancellationTokenSource = new CancellationTokenSource();
            new Thread(() => { ProcessHidReports(_hidCancellationTokenSource.Token); }).Start();
            new Thread(() => { SendHidReports(_hidCancellationTokenSource.Token); }).Start();
        }

        private void DisconnectDevice()
        {
            ThrowIfDisposed();

            _hidCancellationTokenSource?.Cancel();
            _hidCancellationTokenSource?.Dispose();
            _hidCancellationTokenSource = null;

            _hidDevice = null;
            _hidStream?.Close();
            _hidStream = null;
        }

        private IEnumerable<HidDevice> GetRelevantDevices()
        {
            return DeviceList.Local.GetHidDevices()
                .Where(x => x.VendorID == VendorId && ProductIds.Contains(x.ProductID) && GetSerialNumber(x) == _serialNumber);
        }

        private void ProcessHidReports(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            var stopwatch = Stopwatch.StartNew();

            while (!cancellationToken.IsCancellationRequested && _hidDevice != null)
            {
                var report = GetNextReport();
                if (report.Length == 0)
                {
                    continue;
                }

                UpdateStateFromPayload(new HidReportPayload(report, IsBluetooth));

                MeasuredPollingRate.AddSample(stopwatch.Elapsed.TotalMilliseconds);
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

                report.LeftHeavyMotor = (byte)(HeavyMotor.Intensity * 255);
                report.RightLightMotor = (byte)(LightMotor.Intensity * 255);

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
            if (_hidStream == null || _hidDevice == null)
            {
                return new byte[0];
            }
            try
            {
                var buffer = new byte[_hidDevice.GetMaxInputReportLength()];
                if (_hidStream.Read(buffer) > 0)
                {
                    return buffer;
                }
            }
            catch (IOException)
            {
                DisconnectDevice();
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            return new byte[0];
        }

        private void SendReport(byte[] data)
        {
            try
            {
                _hidStream?.Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void UpdateStateFromPayload(HidReportPayload payload)
        {
            Battery.Percentage = payload.BatteryPercentage;
            Battery.IsCharging = payload.Charging;

            Buttons.HeldButtons = payload.HeldButtons;

            LeftJoystick.X = ConvertJoystickValue(payload.LeftJoystickX);
            LeftJoystick.Y = -ConvertJoystickValue(payload.LeftJoystickY);
            RightJoystick.X = ConvertJoystickValue(payload.RightJoystickX);
            RightJoystick.Y = -ConvertJoystickValue(payload.RightJoystickY);

            LeftTrigger.Pressure = payload.LeftTriggerPressure / 255f;
            RightTrigger.Pressure = payload.RightTriggerPressure / 255f;

            OnStateChanged?.Invoke(this, new EventArgs());
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

                    _hidCancellationTokenSource?.Cancel();
                    _hidCancellationTokenSource?.Dispose();
                    _hidCancellationTokenSource = null;

                    Buttons.Dispose();
                    LeftJoystick.Dispose();
                    RightJoystick.Dispose();
                    LeftTrigger.Dispose();
                    RightTrigger.Dispose();

                    _hidDevice = null;
                    _hidStream?.Dispose();
                    _hidStream = null;

                    OnStateChanged = null;
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
