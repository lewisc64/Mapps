using Mapps.Gamepads.Components;

namespace Mapps.Gamepads.Events;

public class JoystickEventArgs : IGamepadEventArgs
{
    public JoystickEventArgs(float x, float y, JoystickPosition position)
    {
        X = x;
        Y = y;
        Position = position;
    }

    public float X { get; set; }

    public float Y { get; set; }

    public JoystickPosition Position { get; }
}
