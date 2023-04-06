using Mapps.Gamepads.Components;
using Mapps.Gamepads.Events;
using Mapps.Gamepads.Input;
using Mapps.Gamepads.Input.Xbox;
using Mapps.Mappers;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Mapps.Gamepads.Output;

public sealed class Xbox360OutputGamepad : IOutputGamepad<XboxButton>
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

    private object _eventSourceLock = new();
    private Action? _detachFromPreviousEventSourceStrategy;
    private IInputGamepad? _feedbackGamepad;

    public Xbox360OutputGamepad()
    {
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

    public void SetFeedbackGamepad(IInputGamepad? gamepad)
    {
        _feedbackGamepad = gamepad;
    }

    public void SetEventSource<TSourceButton>(IGamepadEventSource? eventProducer, ButtonMapper<TSourceButton, XboxButton> buttonMapper)
        where TSourceButton : notnull
    {
        lock (_eventSourceLock)
        {
            _detachFromPreviousEventSourceStrategy?.Invoke();
            _detachFromPreviousEventSourceStrategy = null;

            if (eventProducer is null)
            {
                return;
            }

            _emulatedController?.ResetReport();

            EventHandler<IGamepadEventArgs> gamepadEventListener = (_, gamepadEvent) =>
            {
                ThrowIfDisposed();

                if (_emulatedController == null)
                {
                    return;
                }

                if (gamepadEvent is ButtonEventArgs<TSourceButton> buttonEvent && buttonMapper.HasMapping(buttonEvent.Button))
                {
                    _emulatedController.SetButtonState(ViGEmButtonMap[buttonMapper.Map(buttonEvent.Button)], buttonEvent.IsPressed);
                }

                if (gamepadEvent is TriggerEventArgs triggerEvent)
                {
                    switch (triggerEvent.Position)
                    {
                        case TriggerPosition.Left:
                            _emulatedController.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)(255 * triggerEvent.Value));
                            break;
                        case TriggerPosition.Right:
                            _emulatedController.SetSliderValue(Xbox360Slider.RightTrigger, (byte)(255 * triggerEvent.Value));
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported trigger position: ${triggerEvent.Position}");
                    }
                }

                if (gamepadEvent is JoystickEventArgs joystickEvent)
                {
                    switch (joystickEvent.Position)
                    {
                        case JoystickPosition.Left:
                            _emulatedController.SetAxisValue(Xbox360Axis.LeftThumbX, TransformAxis(joystickEvent.X));
                            _emulatedController.SetAxisValue(Xbox360Axis.LeftThumbY, TransformAxis(joystickEvent.Y));
                            break;
                        case JoystickPosition.Right:
                            _emulatedController.SetAxisValue(Xbox360Axis.RightThumbX, TransformAxis(joystickEvent.X));
                            _emulatedController.SetAxisValue(Xbox360Axis.RightThumbY, TransformAxis(joystickEvent.Y));
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported joystick position: ${joystickEvent.Position}");
                    }
                }

                _emulatedController.SubmitReport();
            };

            eventProducer.EventDispatch += gamepadEventListener;
            _detachFromPreviousEventSourceStrategy = () => eventProducer.EventDispatch -= gamepadEventListener;
        }
    }

    public void DetachEventSource()
    {
        lock (_eventSourceLock)
        {
            _detachFromPreviousEventSourceStrategy?.Invoke();
            _detachFromPreviousEventSourceStrategy = null;
        }
    }

    private static short TransformAxis(float value)
    {
        return (short)(value * short.MaxValue);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _detachFromPreviousEventSourceStrategy?.Invoke();
                _detachFromPreviousEventSourceStrategy = null;
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
            throw new ObjectDisposedException(nameof(Xbox360OutputGamepad));
        }
    }
}
