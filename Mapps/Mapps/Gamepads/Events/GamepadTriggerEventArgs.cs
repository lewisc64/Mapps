namespace Mapps.Gamepads.Events;

public class GamepadTriggerEventArgs : IGamepadEventArgs
{
    public GamepadTriggerEventArgs(float value, GamepadEventTriggerPosition position)
    {
        Value = value;
        Position = position;
    }

    public float Value { get; }

    public GamepadEventTriggerPosition Position { get; }
}
