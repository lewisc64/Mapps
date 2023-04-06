using Mapps.Gamepads.Components;

namespace Mapps.Gamepads.Input;

public interface IInputGamepad : IDisposable
{
    event EventHandler? OnConnect;
    event EventHandler? OnDisconnect;
    event EventHandler? OnStateChanged;

    bool IsTracking { get; }

    bool IsConnected { get; }

    void StartTracking();

    void StopTracking();
}

public interface IHasButtons<T>
    where T : notnull
{
    Buttons<T> Buttons { get; }
}

public interface IHasDualJoysticks
{
    Joystick LeftJoystick { get; }

    Joystick RightJoystick { get; }
}

public interface IHasDualTriggers
{
    Trigger LeftTrigger { get; }

    Trigger RightTrigger { get; }
}

public interface IHasTwoDistinctMassRumbleMotors
{
    RumbleMotor HeavyMotor { get; }

    RumbleMotor LightMotor { get; }

    Task TestRumble();
}

public interface IHasBattery
{
    Battery Battery { get; }
}
