namespace Mapps.Gamepads.Components;

public class RumbleMotor : IGamepadComponent
{
    public RumbleMotor()
    {
    }

    public float Intensity { get; set; } = 0.0f;
}
