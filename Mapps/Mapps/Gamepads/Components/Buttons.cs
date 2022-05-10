namespace Mapps.Gamepads.Components
{
    public class Buttons<T> : IGamepadComponent
    {
        private IEnumerable<T> _heldButtons = new List<T>();

        public Buttons()
        {
        }

        public event EventHandler<T>? OnButtonDown;

        public event EventHandler<T>? OnButtonUp;

        public IEnumerable<T> HeldButtons
        {
            get
            {
                return _heldButtons;
            }

            internal set
            {
                foreach (var button in _heldButtons.Except(value))
                {
                    Task.Run(() =>
                    {
                        OnButtonUp?.Invoke(this, button);
                    });
                }

                foreach (var button in value.Except(_heldButtons))
                {
                    Task.Run(() =>
                    {
                        OnButtonDown?.Invoke(this, button);
                    });
                }

                _heldButtons = value;
            }
        }

        public bool IsPressed(T button)
        {
            return HeldButtons.Contains(button);
        }
    }
}
