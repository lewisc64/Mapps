namespace Mapps.Gamepads.Events;

public class GamepadJoystickEventArgs : IGamepadEventArgs
{
    public GamepadJoystickEventArgs(float x, float y, GamepadEventJoystickPosition position)
    {
        X = x;
        Y = y;
        Position = position;
    }

    public float X { get; set; }

    public float Y { get; set; }

    public GamepadEventJoystickPosition Position { get; }
}
