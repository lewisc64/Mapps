namespace Mapps.Gamepads.Components;

public class Buttons<T> : IGamepadComponent
    where T : notnull
{
    private IEnumerable<T> _heldButtons = new List<T>();

    public event EventHandler<T>? OnButtonDown;
    public event EventHandler<T>? OnButtonUp;

    public Buttons()
    {
    }

    public IEnumerable<T> HeldButtons
    {
        get
        {
            return _heldButtons;
        }

        internal set
        {
            var previous = _heldButtons;
            _heldButtons = value;

            foreach (var button in previous.Except(value))
            {
                Task.Run(() =>
                {
                    OnButtonUp?.Invoke(this, button);
                });
            }

            foreach (var button in value.Except(previous))
            {
                Task.Run(() =>
                {
                    OnButtonDown?.Invoke(this, button);
                });
            }
        }
    }

    public bool IsPressed(T button)
    {
        return HeldButtons.Contains(button);
    }
}
