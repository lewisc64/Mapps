namespace Mapps.Gamepads.Components
{
    public class Joystick : IGamepadComponent
    {
        public byte X { get; internal set; }

        public byte Y { get; internal set; }

        public Joystick()
        {
        }
    }
}
