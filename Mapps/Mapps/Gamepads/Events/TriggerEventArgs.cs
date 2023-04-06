using Mapps.Gamepads.Components;

namespace Mapps.Gamepads.Events;

public class TriggerEventArgs : IGamepadEventArgs
{
    public TriggerEventArgs(float value, TriggerPosition position)
    {
        Value = value;
        Position = position;
    }

    public float Value { get; }

    public TriggerPosition Position { get; }
}
