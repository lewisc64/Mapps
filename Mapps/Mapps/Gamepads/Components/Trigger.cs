namespace Mapps.Gamepads.Components
{
    public class Trigger : IGamepadComponent
    {
        public byte Pressure { get; internal set; }

        public bool Active { get; internal set; }

        public Trigger()
        {
        }
    }
}
