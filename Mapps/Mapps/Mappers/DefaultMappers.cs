using Mapps.Gamepads.DualShock4;
using Mapps.Gamepads.XInput;

namespace Mapps.Mappers
{
    public static class DefaultMappers
    {
        public static ButtonMapper<DS4Button, XboxButton> DualShock4ToXboxButtonMapper
        {
            get
            {
                var mapper = new ButtonMapper<DS4Button, XboxButton>();

                mapper.SetMapping(DS4Button.DpadUp, XboxButton.DpadUp);
                mapper.SetMapping(DS4Button.DpadDown, XboxButton.DpadDown);
                mapper.SetMapping(DS4Button.DpadLeft, XboxButton.DpadLeft);
                mapper.SetMapping(DS4Button.DpadRight, XboxButton.DpadRight);

                mapper.SetMapping(DS4Button.Cross, XboxButton.A);
                mapper.SetMapping(DS4Button.Circle, XboxButton.B);
                mapper.SetMapping(DS4Button.Square, XboxButton.X);
                mapper.SetMapping(DS4Button.Triangle, XboxButton.Y);

                mapper.SetMapping(DS4Button.L1, XboxButton.LeftShoulder);
                mapper.SetMapping(DS4Button.R1, XboxButton.RightShoulder);

                mapper.SetMapping(DS4Button.L3, XboxButton.LeftStick);
                mapper.SetMapping(DS4Button.R3, XboxButton.RightStick);

                mapper.SetMapping(DS4Button.Share, XboxButton.Back);
                mapper.SetMapping(DS4Button.Options, XboxButton.Start);
                mapper.SetMapping(DS4Button.PS, XboxButton.Guide);

                return mapper;
            }
        }
    }
}
