namespace Mapps.Gamepads.Components;

public class Battery : IGamepadComponent
{
    private int _percentage;
    private bool _charging;

    public event EventHandler<int>? OnLevelChanged;
    public event EventHandler? OnCharging;
    public event EventHandler? OnDischarging;

    public Battery()
    {
    }

    public int Percentage
    {
        get
        {
            return _percentage;
        }

        set
        {
            var previous = _percentage;
            _percentage = Math.Max(Math.Min(value, 100), 0);
            if (previous != _percentage)
            {
                Task.Run(() =>
                {
                    OnLevelChanged?.Invoke(this, _percentage);
                });
            }
        }
    }

    public bool IsCharging
    {
        get
        {
            return _charging;
        }

        set
        {
            var previous = _charging;
            _charging = value;
            if (previous != _charging)
            {
                Task.Run(() =>
                {
                    (_charging ? OnCharging : OnDischarging)?.Invoke(this, EventArgs.Empty);
                });
            }
        }
    }
}
