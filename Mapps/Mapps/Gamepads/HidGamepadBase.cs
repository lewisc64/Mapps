using HidSharp;
using Mapps.Trackers;
using System.Diagnostics;

namespace Mapps.Gamepads
{
    public abstract class HidGamepadBase : IGamepad
    {
        private static readonly TimeSpan DeviceCheckInterval = TimeSpan.FromMilliseconds(500);

        private bool _disposed;

        private HidDevice? _hidDevice;

        private HidStream? _hidStream;

        private CancellationTokenSource? _cancellationTokenSource = null;

        private CancellationTokenSource? _hidCancellationTokenSource = null;

        public bool IsTracking { get; private set; }

        public bool IsConnected { get; private set; }

        public NumberTracker MeasuredPollingRate { get; } = new NumberTracker(500);

        public HidDevice? ActiveHidDevice => _hidDevice;

        protected virtual TimeSpan OutputReportInterval => TimeSpan.Zero;

        public event EventHandler? OnConnect;

        public event EventHandler? OnDisconnect;

        public event EventHandler? OnStateChanged;

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

        public abstract Task TestRumble();

        protected abstract IEnumerable<HidDevice> GetRelevantDevicesByPriority();

        protected abstract void ProcessInputReport(byte[] report);

        protected abstract byte[] GenerateOutputReport();

        protected abstract void DisposeComponents();

        private void ManageDevices(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var desiredDevice = GetRelevantDevicesByPriority().FirstOrDefault();

                    if (desiredDevice != null && (_hidDevice == null || _hidDevice.DevicePath != desiredDevice.DevicePath))
                    {
                        if (IsConnected)
                        {
                            DisconnectDevice();
                        }
                        ConnectDevice(desiredDevice);
                    }

                    if (desiredDevice == null && IsConnected)
                    {
                        DisconnectDevice();
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

                    Thread.Sleep(DeviceCheckInterval);
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

        private void ConnectDevice(HidDevice device)
        {
            ThrowIfDisposed();

            _hidDevice = device;
            _hidStream = _hidDevice.Open();

            _hidCancellationTokenSource = new CancellationTokenSource();
            new Thread(() => { RecieveHidReports(_hidCancellationTokenSource.Token); }).Start();
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

        private void RecieveHidReports(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            var pollingRateStopwatch = Stopwatch.StartNew();

            try
            {
                while (!cancellationToken.IsCancellationRequested && _hidDevice != null)
                {
                    var report = GetNextReport();
                    if (report.Length == 0)
                    {
                        continue;
                    }

                    ProcessInputReport(report);
                    OnStateChanged?.Invoke(this, new EventArgs());

                    MeasuredPollingRate.AddSample(pollingRateStopwatch.Elapsed.TotalMilliseconds);
                    pollingRateStopwatch.Restart();
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
        }

        private void SendHidReports(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            try
            {
                while (!cancellationToken.IsCancellationRequested && _hidDevice != null)
                {
                    SendReport(GenerateOutputReport());
                    if (OutputReportInterval.TotalMilliseconds > 0)
                    {
                        Thread.Sleep(OutputReportInterval);
                    }
                }
            }
            catch (IOException)
            {
                // ignore
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }

        private byte[] GetNextReport()
        {
            if (_hidStream == null || _hidDevice == null)
            {
                return new byte[0];
            }
            var buffer = new byte[_hidDevice.GetMaxInputReportLength()];
            if (_hidStream.Read(buffer) > 0)
            {
                return buffer;
            }
            return new byte[0];
        }

        private void SendReport(byte[] data)
        {
            try
            {
                _hidStream?.Write(data, 0, data.Length);
            }
            catch (TimeoutException)
            {
                // ignore
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    OnStateChanged = null;

                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;

                    _hidCancellationTokenSource?.Cancel();
                    _hidCancellationTokenSource?.Dispose();
                    _hidCancellationTokenSource = null;

                    _hidDevice = null;
                    _hidStream?.Dispose();
                    _hidStream = null;

                    DisposeComponents();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
