using Mapps.Gamepads;
using Mapps.Mappers;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Mapps.OutputWrappers
{
    public class ToXbox360<TButton> : IGamepadOutputWrapper, IDisposable
        where TButton : notnull
    {
        private bool _disposed;

        private readonly ViGEmClient _client;

        private IXbox360Controller? _emulatedController;

        private IGamepad _gamepad;

        private ButtonMapper<TButton, Xbox360Button> _buttonMapper;

        private EventHandler _stateChangedListener;

        public ToXbox360(IGamepad gamepad, ButtonMapper<TButton, Xbox360Button> buttonMapper)
        {
            _gamepad = gamepad;
            _buttonMapper = buttonMapper;
            _client = new ViGEmClient();

            _stateChangedListener = (a, b) =>
            {
                ThrowIfDisposed();

                if (_emulatedController == null)
                {
                    return;
                }

                if (gamepad is IHasButtons<TButton> buttons)
                {
                    foreach (var button in _buttonMapper.MappedButtons)
                    {
                        _emulatedController.SetButtonState(_buttonMapper.Map(button), buttons.Buttons.IsPressed(button));
                    }
                }

                if (gamepad is IHasDualTriggers triggers)
                {
                    _emulatedController.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)(255 * triggers.LeftTrigger.Pressure));
                    _emulatedController.SetSliderValue(Xbox360Slider.RightTrigger, (byte)(255 * triggers.RightTrigger.Pressure));
                }

                if (gamepad is IHasDualJoysticks joysticks)
                {
                    _emulatedController.SetAxisValue(Xbox360Axis.LeftThumbX, TransformAxis(joysticks.LeftJoystick.X));
                    _emulatedController.SetAxisValue(Xbox360Axis.LeftThumbY, TransformAxis(joysticks.LeftJoystick.Y));
                    _emulatedController.SetAxisValue(Xbox360Axis.RightThumbX, TransformAxis(joysticks.RightJoystick.X));
                    _emulatedController.SetAxisValue(Xbox360Axis.RightThumbY, TransformAxis(joysticks.RightJoystick.Y));
                }

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
                if (_gamepad is IHasTwoDistinctMassRumbleMotors motors)
                {
                    motors.HeavyMotor.Intensity = evnt.LargeMotor / 255f;
                    motors.LightMotor.Intensity = evnt.SmallMotor / 255f;
                }
            };

            _gamepad.OnStateChanged += _stateChangedListener;

            IsConnected = true;
        }

        public void Disconnect()
        {
            ThrowIfDisposed();

            _gamepad.OnStateChanged -= _stateChangedListener;

            _emulatedController?.Disconnect();
            _emulatedController = null;

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
                throw new ObjectDisposedException(nameof(ToXbox360<TButton>));
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
