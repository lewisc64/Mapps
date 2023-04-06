namespace Mapps.Gamepads.Components;

public class Trigger : IGamepadComponent
{
    private float _pressure;

    public event EventHandler<float>? OnChange;

    public Trigger()
    {
    }

    public float Pressure
    {
        get
        {
            return _pressure < DeadZone ? 0 : _pressure;
        }

        internal set
        {
            var previous = Pressure;
            _pressure = value;
            var now = Pressure;

            if (previous != now)
            {
                Task.Run(() =>
                {
                    OnChange?.Invoke(this, now);
                });
            }
        }
    }

    public float DeadZone { get; set; } = 0.0f;
}
