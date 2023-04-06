namespace Mapps.Gamepads.Events;

public class GamepadButtonEventArgs<TButton> : IGamepadEventArgs
{
    public GamepadButtonEventArgs(TButton button, bool isPressed)
    {
        Button = button;
        IsPressed = isPressed;
    }

    public TButton Button { get; }

    public bool IsPressed { get; }
}
