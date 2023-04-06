namespace Mapps.Gamepads.Events;

public class ButtonEventArgs<TButton> : IGamepadEventArgs
    where TButton : notnull
{
    public ButtonEventArgs(TButton button, bool isPressed)
    {
        Button = button;
        IsPressed = isPressed;
    }

    public TButton Button { get; }

    public bool IsPressed { get; }
}
