namespace Mapps.Gamepads.Components
{
    public class Buttons<T> : IGamepadComponent
    {
        public IEnumerable<T> HeldButtons { get; internal set; } = new List<T>();

        public Buttons()
        {
        }

        public bool IsPressed(T button)
        {
            return HeldButtons.Contains(button);
        }
    }
}
