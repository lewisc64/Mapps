namespace Mapps.Gamepads.Components
{
    public class Joystick : IGamepadComponent
    {
        private float _x;

        private float _y;

        public Joystick()
        {
        }

        public float X
        {
            get
            {
                return ApplyDeadZone(_x);
            }

            internal set
            {
                _x = value;
            }
        }

        public float Y
        {
            get
            {
                return ApplyDeadZone(_y);
            }

            internal set
            {
                _y = value;
            }
        }

        public float DeadZone { get; set; } = 0.1f;

        private float ApplyDeadZone(float value)
        {
            return Math.Pow(_x, 2) + Math.Pow(_y, 2) < Math.Pow(DeadZone, 2) ? 0 : value;
        }
    }
}
