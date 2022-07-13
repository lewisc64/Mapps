using Mapps.Gamepads;
using Mapps.Gamepads.Styles.Xbox;
using Mapps.Mappers;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Mapps.OutputWrappers
{
    public class ToXbox360<TButton> : IGamepadOutputWrapper, IDisposable
        where TButton : notnull
    {
        private static readonly Dictionary<XboxButton, Xbox360Button> ViGEmButtonMap = new()
        {
            { XboxButton.A, Xbox360Button.A },
            { XboxButton.B, Xbox360Button.B },
            { XboxButton.X, Xbox360Button.X },
            { XboxButton.Y, Xbox360Button.Y },
            { XboxButton.DpadUp, Xbox360Button.Up },
            { XboxButton.DpadDown, Xbox360Button.Down },
            { XboxButton.DpadLeft, Xbox360Button.Left },
            { XboxButton.DpadRight, Xbox360Button.Right },
            { XboxButton.RightShoulder, Xbox360Button.RightShoulder },
            { XboxButton.LeftShoulder, Xbox360Button.LeftShoulder },
            { XboxButton.LeftStick, Xbox360Button.LeftThumb },
            { XboxButton.RightStick, Xbox360Button.RightThumb },
            { XboxButton.Back, Xbox360Button.Back },
            { XboxButton.Start, Xbox360Button.Start },
            { XboxButton.Guide, Xbox360Button.Guide },
        };

        private bool _disposed;

        private readonly ViGEmClient _client;

        private IXbox360Controller? _emulatedController;

        private readonly IGamepad _gamepad;

        private readonly EventHandler _stateChangedListener;

        public ToXbox360(IGamepad gamepad, ButtonMapper<TButton, XboxButton> buttonMapper)
        {
            _gamepad = gamepad;
            ButtonMapper = buttonMapper;
            _client = new ViGEmClient();

            _stateChangedListener = (a, b) =>
            {
                ThrowIfDisposed();

                if (_emulatedController == null)
                {
                    return;
                }

                if (_gamepad is IHasButtons<TButton> buttons)
                {
                    foreach (var button in ButtonMapper.MappedButtons)
                    {
                        _emulatedController.SetButtonState(ViGEmButtonMap[ButtonMapper.Map(button)], buttons.Buttons.IsPressed(button));
                    }
                }

                if (_gamepad is IHasDualTriggers triggers)
                {
                    _emulatedController.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)(255 * triggers.LeftTrigger.Pressure));
                    _emulatedController.SetSliderValue(Xbox360Slider.RightTrigger, (byte)(255 * triggers.RightTrigger.Pressure));
                }

                if (_gamepad is IHasDualJoysticks joysticks)
                {
                    _emulatedController.SetAxisValue(Xbox360Axis.LeftThumbX, TransformAxis(joysticks.LeftJoystick.X));
                    _emulatedController.SetAxisValue(Xbox360Axis.LeftThumbY, TransformAxis(joysticks.LeftJoystick.Y));
                    _emulatedController.SetAxisValue(Xbox360Axis.RightThumbX, TransformAxis(joysticks.RightJoystick.X));
                    _emulatedController.SetAxisValue(Xbox360Axis.RightThumbY, TransformAxis(joysticks.RightJoystick.Y));
                }

                _emulatedController.SubmitReport();
            };
        }

        public ButtonMapper<TButton, XboxButton> ButtonMapper { get; set; }

        public bool IsConnected { get; private set; }

        public void Connect()
        {
            ThrowIfDisposed();

            if (IsConnected)
            {
                throw new InvalidOperationException("Already connected.");
            }

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
    }
}
