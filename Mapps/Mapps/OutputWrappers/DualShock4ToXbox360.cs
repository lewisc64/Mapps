using Mapps.Gamepads.DualShock4;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Mapps.OutputWrappers
{
    public class DualShock4ToXbox360 : IGamepadOutputWrapper, IDisposable
    {
        private bool _disposed;

        private readonly ViGEmClient _client;

        private IXbox360Controller? _emulatedController;

        private DualShock4 _gamepad;

        EventHandler _stateChangedListener;

        public DualShock4ToXbox360(DualShock4 gamepad)
        {
            _gamepad = gamepad;

            _client = new ViGEmClient();

            _stateChangedListener = (a, b) =>
            {
                ThrowIfDisposed();

                if (_emulatedController == null || _gamepad == null)
                {
                    return;
                }

                _emulatedController.SetButtonState(Xbox360Button.A, _gamepad.Buttons.IsPressed(DS4Button.Cross));
                _emulatedController.SetButtonState(Xbox360Button.B, _gamepad.Buttons.IsPressed(DS4Button.Circle));
                _emulatedController.SetButtonState(Xbox360Button.X, _gamepad.Buttons.IsPressed(DS4Button.Square));
                _emulatedController.SetButtonState(Xbox360Button.Y, _gamepad.Buttons.IsPressed(DS4Button.Triangle));
                _emulatedController.SetButtonState(Xbox360Button.Back, _gamepad.Buttons.IsPressed(DS4Button.Share));
                _emulatedController.SetButtonState(Xbox360Button.Start, _gamepad.Buttons.IsPressed(DS4Button.Options));
                _emulatedController.SetButtonState(Xbox360Button.LeftShoulder, _gamepad.Buttons.IsPressed(DS4Button.L1));
                _emulatedController.SetButtonState(Xbox360Button.RightShoulder, _gamepad.Buttons.IsPressed(DS4Button.R1));
                _emulatedController.SetButtonState(Xbox360Button.Guide, _gamepad.Buttons.IsPressed(DS4Button.PS));
                _emulatedController.SetButtonState(Xbox360Button.LeftThumb, _gamepad.Buttons.IsPressed(DS4Button.L3));
                _emulatedController.SetButtonState(Xbox360Button.RightThumb, _gamepad.Buttons.IsPressed(DS4Button.R3));

                _emulatedController.SetButtonState(Xbox360Button.Up, _gamepad.Buttons.IsPressed(DS4Button.DpadUp));
                _emulatedController.SetButtonState(Xbox360Button.Down, _gamepad.Buttons.IsPressed(DS4Button.DpadDown));
                _emulatedController.SetButtonState(Xbox360Button.Left, _gamepad.Buttons.IsPressed(DS4Button.DpadLeft));
                _emulatedController.SetButtonState(Xbox360Button.Right, _gamepad.Buttons.IsPressed(DS4Button.DpadRight));

                _emulatedController.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)(255 * _gamepad.LeftTrigger.Pressure));
                _emulatedController.SetSliderValue(Xbox360Slider.RightTrigger, (byte)(255 * _gamepad.RightTrigger.Pressure));

                _emulatedController.SetAxisValue(Xbox360Axis.LeftThumbX, TransformAxis(_gamepad.LeftJoystick.X));
                _emulatedController.SetAxisValue(Xbox360Axis.LeftThumbY, TransformAxis(_gamepad.LeftJoystick.Y));
                _emulatedController.SetAxisValue(Xbox360Axis.RightThumbX, TransformAxis(_gamepad.RightJoystick.X));
                _emulatedController.SetAxisValue(Xbox360Axis.RightThumbY, TransformAxis(_gamepad.RightJoystick.Y));

                _emulatedController.SubmitReport();
            };
        }

        public bool IsConnected { get; private set; }

        public void Connect()
        {
            ThrowIfDisposed();

            _emulatedController = _client.CreateXbox360Controller();
            _emulatedController.Connect();

            _emulatedController.FeedbackReceived += (_, evnt) =>
            {
                _gamepad.LeftHeavyMotor.Intensity = evnt.LargeMotor / 255f;
                _gamepad.RightLightMotor.Intensity = evnt.SmallMotor / 255f;
            };

            _gamepad.OnStateChanged += _stateChangedListener;

            IsConnected = true;
        }

        public void Disconnect()
        {
            ThrowIfDisposed();

            _emulatedController?.Disconnect();
            _emulatedController = null;

            _gamepad.OnStateChanged -= _stateChangedListener;

            IsConnected = false;
        }

        private short TransformAxis(float value)
        {
            return (short)(value * short.MaxValue);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                    _client.Dispose();
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
                throw new ObjectDisposedException(nameof(DualShock4ToXbox360));
            }
        }

        private void ThrowIfNotConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Emulated controller is not connected.");
            }
        }
    }
}
