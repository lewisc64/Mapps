using Mapps.Gamepads.Components;

namespace Mapps.Gamepads.Events;

public interface IGamepadEventProducer<TButton> : IDisposable
{
    event EventHandler<IGamepadEventArgs>? EventDispatch;

    void Register(IGamepad gamepad);

    void Deregister();
}

public class GamepadEventProducer<TButton> : IGamepadEventProducer<TButton>
{
    private bool _disposed;
    private List<Action> ListenerDestroyers = new();

    public event EventHandler<IGamepadEventArgs>? EventDispatch;

    public GamepadEventProducer()
    {
    }

    public void Register(IGamepad gamepad)
    {
        DestroyListeners();
        CreateListeners(gamepad);

    }

    public void Deregister()
    {
        DestroyListeners();
    }

    private void CreateListeners(IGamepad gamepad)
    {
        CreateButtonListeners(gamepad);
        CreateJoystickListeners(gamepad);
        CreateTriggerListeners(gamepad);
    }

    private void CreateButtonListeners(IGamepad gamepad)
    {
        if (gamepad is IHasButtons<TButton> gamepadWithButtons)
        {
            EventHandler<TButton> buttonDownListener = (_, button) =>
            {
                EventDispatch?.Invoke(this, new GamepadButtonEventArgs<TButton>(button, true));
            };

            EventHandler<TButton> buttonUpListener = (_, button) =>
            {
                EventDispatch?.Invoke(this, new GamepadButtonEventArgs<TButton>(button, false));
            };

            gamepadWithButtons.Buttons.OnButtonDown += buttonDownListener;
            ListenerDestroyers.Add(() => gamepadWithButtons.Buttons.OnButtonDown -= buttonDownListener);

            gamepadWithButtons.Buttons.OnButtonUp += buttonUpListener;
            ListenerDestroyers.Add(() => gamepadWithButtons.Buttons.OnButtonDown -= buttonUpListener);
        }
    }

    private void CreateJoystickListeners(IGamepad gamepad)
    {
        if (gamepad is IHasDualJoysticks gamepadWithJoysticks)
        {
            CreateListenersForJoystick(gamepad, gamepadWithJoysticks.LeftJoystick, GamepadEventJoystickPosition.Left);
            CreateListenersForJoystick(gamepad, gamepadWithJoysticks.RightJoystick, GamepadEventJoystickPosition.Right);
        }
    }

    private void CreateListenersForJoystick(IGamepad gamepad, Joystick joystick, GamepadEventJoystickPosition position)
    {
        GamepadJoystickEventArgs? previousEvent = null;
        EventHandler handler = (a, b) =>
        {
            if (previousEvent is null || previousEvent.X != joystick.X || previousEvent.Y != joystick.Y)
            {
                var currentEvent = new GamepadJoystickEventArgs(joystick.X, joystick.Y, position);
                EventDispatch?.Invoke(this, currentEvent);
                previousEvent = currentEvent;
            }
        };

        gamepad.OnStateChanged += handler;
        ListenerDestroyers.Add(() => gamepad.OnStateChanged -= handler);
    }

    private void CreateTriggerListeners(IGamepad gamepad)
    {
        if (gamepad is IHasDualTriggers gamepadWithTriggers)
        {
            CreateListenersForTrigger(gamepad, gamepadWithTriggers.LeftTrigger, GamepadEventTriggerPosition.Left);
            CreateListenersForTrigger(gamepad, gamepadWithTriggers.RightTrigger, GamepadEventTriggerPosition.Right);
        }
    }

    private void CreateListenersForTrigger(IGamepad gamepad, Trigger trigger, GamepadEventTriggerPosition position)
    {
        EventHandler<float> handler = (_, value) =>
        {
            EventDispatch?.Invoke(this, new GamepadTriggerEventArgs(value, position));
        };

        trigger.OnChange += handler;
        ListenerDestroyers.Add(() => trigger.OnChange -= handler);
    }

    private void DestroyListeners()
    {
        foreach (var action in ListenerDestroyers)
        {
            action.Invoke();
        }
        ListenerDestroyers.Clear();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DestroyListeners();
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
