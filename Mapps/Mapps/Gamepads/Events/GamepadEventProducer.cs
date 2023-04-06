using Mapps.Gamepads.Components;
using Mapps.Gamepads.Input;

namespace Mapps.Gamepads.Events;

public class GamepadEventProducer<TButton> : IGamepadEventSource
    where TButton : notnull
{
    private bool _disposed;
    private List<Action> ListenerDestroyers = new();

    public event EventHandler<IGamepadEventArgs>? EventDispatch;

    public GamepadEventProducer()
    {
    }

    public void Register(IInputGamepad gamepad)
    {
        DestroyListeners();
        CreateListeners(gamepad);

    }

    public void Deregister()
    {
        DestroyListeners();
    }

    private void CreateListeners(IInputGamepad gamepad)
    {
        CreateButtonListeners(gamepad);
        CreateJoystickListeners(gamepad);
        CreateTriggerListeners(gamepad);
    }

    private void CreateButtonListeners(IInputGamepad gamepad)
    {
        if (gamepad is IHasButtons<TButton> gamepadWithButtons)
        {
            EventHandler<TButton> buttonDownListener = (_, button) =>
            {
                EventDispatch?.Invoke(this, new ButtonEventArgs<TButton>(button, true));
            };

            EventHandler<TButton> buttonUpListener = (_, button) =>
            {
                EventDispatch?.Invoke(this, new ButtonEventArgs<TButton>(button, false));
            };

            gamepadWithButtons.Buttons.OnButtonDown += buttonDownListener;
            ListenerDestroyers.Add(() => gamepadWithButtons.Buttons.OnButtonDown -= buttonDownListener);

            gamepadWithButtons.Buttons.OnButtonUp += buttonUpListener;
            ListenerDestroyers.Add(() => gamepadWithButtons.Buttons.OnButtonDown -= buttonUpListener);
        }
    }

    private void CreateJoystickListeners(IInputGamepad gamepad)
    {
        if (gamepad is IHasDualJoysticks gamepadWithJoysticks)
        {
            CreateListenersForJoystick(gamepad, gamepadWithJoysticks.LeftJoystick, JoystickPosition.Left);
            CreateListenersForJoystick(gamepad, gamepadWithJoysticks.RightJoystick, JoystickPosition.Right);
        }
    }

    private void CreateListenersForJoystick(IInputGamepad gamepad, Joystick joystick, JoystickPosition position)
    {
        JoystickEventArgs? previousEvent = null;
        EventHandler handler = (a, b) =>
        {
            if (previousEvent is null || previousEvent.X != joystick.X || previousEvent.Y != joystick.Y)
            {
                var currentEvent = new JoystickEventArgs(joystick.X, joystick.Y, position);
                EventDispatch?.Invoke(this, currentEvent);
                previousEvent = currentEvent;
            }
        };

        gamepad.OnStateChanged += handler;
        ListenerDestroyers.Add(() => gamepad.OnStateChanged -= handler);
    }

    private void CreateTriggerListeners(IInputGamepad gamepad)
    {
        if (gamepad is IHasDualTriggers gamepadWithTriggers)
        {
            CreateListenersForTrigger(gamepad, gamepadWithTriggers.LeftTrigger, TriggerPosition.Left);
            CreateListenersForTrigger(gamepad, gamepadWithTriggers.RightTrigger, TriggerPosition.Right);
        }
    }

    private void CreateListenersForTrigger(IInputGamepad gamepad, Trigger trigger, TriggerPosition position)
    {
        EventHandler<float> handler = (_, value) =>
        {
            EventDispatch?.Invoke(this, new TriggerEventArgs(value, position));
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
