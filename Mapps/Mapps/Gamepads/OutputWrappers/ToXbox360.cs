using Mapps.Gamepads.Events;
using Mapps.Gamepads.Styles.Xbox;
using Mapps.Mappers;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Mapps.Gamepads.OutputWrappers
{
    public class ToXbox360<TButton> : IGamepadOutputWrapper<TButton>
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
        private readonly ButtonMapper<TButton, XboxButton> _buttonMapper;
        private IXbox360Controller? _emulatedController;

        private EventHandler<IGamepadEventArgs>? _gamepadEventListener;
        private IGamepadEventProducer<TButton>? _eventProducer;
        private IGamepad? _feedbackGamepad;

        public ToXbox360(ButtonMapper<TButton, XboxButton> buttonMapper)
        {
            _buttonMapper = buttonMapper;
            _client = new ViGEmClient();
        }

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
                if (_feedbackGamepad is not null && _feedbackGamepad is IHasTwoDistinctMassRumbleMotors motors)
                {
                    motors.HeavyMotor.Intensity = evnt.LargeMotor / 255f;
                    motors.LightMotor.Intensity = evnt.SmallMotor / 255f;
                }
            };

            IsConnected = true;
        }

        public void Disconnect()
        {
            ThrowIfDisposed();

            _emulatedController?.Disconnect();
            _emulatedController = null;

            IsConnected = false;
        }

        public void SetFeedbackGamepad(IGamepad? gamepad)
        {
            _feedbackGamepad = gamepad;
        }

        public void SetEventSource(IGamepadEventProducer<TButton>? eventProducer)
        {
            if (_eventProducer is not null && _gamepadEventListener is not null)
            {
                _eventProducer.EventDispatch -= _gamepadEventListener;
            }

            if (eventProducer is null)
            {
                _eventProducer = null;
                _gamepadEventListener = null;
                return;
            }

            _emulatedController?.ResetReport();

            _eventProducer = eventProducer;
            _gamepadEventListener = (_, gamepadEvent) =>
            {
                ThrowIfDisposed();

                if (_emulatedController == null)
                {
                    return;
                }

                if (gamepadEvent is GamepadButtonEventArgs<TButton> buttonEvent && _buttonMapper.HasMapping(buttonEvent.Button))
                {
                    _emulatedController.SetButtonState(ViGEmButtonMap[_buttonMapper.Map(buttonEvent.Button)], buttonEvent.IsPressed);
                }

                if (gamepadEvent is GamepadTriggerEventArgs triggerEvent)
                {
                    switch (triggerEvent.Position)
                    {
                        case GamepadEventTriggerPosition.Left:
                            _emulatedController.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)(255 * triggerEvent.Value));
                            break;
                        case GamepadEventTriggerPosition.Right:
                            _emulatedController.SetSliderValue(Xbox360Slider.RightTrigger, (byte)(255 * triggerEvent.Value));
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported trigger position: ${triggerEvent.Position}");
                    }
                }

                if (gamepadEvent is GamepadJoystickEventArgs joystickEvent)
                {
                    switch (joystickEvent.Position)
                    {
                        case GamepadEventJoystickPosition.Left:
                            _emulatedController.SetAxisValue(Xbox360Axis.LeftThumbX, TransformAxis(joystickEvent.X));
                            _emulatedController.SetAxisValue(Xbox360Axis.LeftThumbY, TransformAxis(joystickEvent.Y));
                            break;
                        case GamepadEventJoystickPosition.Right:
                            _emulatedController.SetAxisValue(Xbox360Axis.RightThumbX, TransformAxis(joystickEvent.X));
                            _emulatedController.SetAxisValue(Xbox360Axis.RightThumbY, TransformAxis(joystickEvent.Y));
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported joystick position: ${joystickEvent.Position}");
                    }
                }

                _emulatedController.SubmitReport();
            };

            _eventProducer.EventDispatch += _gamepadEventListener;
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
                    if (_eventProducer is not null)
                    {
                        _eventProducer.EventDispatch -= _gamepadEventListener;
                        _eventProducer = null;
                    }
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
