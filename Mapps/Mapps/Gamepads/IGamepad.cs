using Mapps.Gamepads.Components;

namespace Mapps.Gamepads
{
    public interface IGamepad : IDisposable
    {
        event EventHandler? OnConnect;

        event EventHandler? OnDisconnect;

        public event EventHandler? OnStateChanged;

        bool IsTracking { get; }

        bool IsConnected { get; }

        void StartTracking();

        void StopTracking();
    }

    public interface IHasButtons<T>
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
}
