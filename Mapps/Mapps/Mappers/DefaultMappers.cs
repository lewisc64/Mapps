using Mapps.Gamepads.Input.Playstation;
using Mapps.Gamepads.Input.Xbox;

namespace Mapps.Mappers;

public static class DefaultMappers
{
    public static ButtonMapper<PSButton, XboxButton> DualShock4ToXboxButtonMapper
    {
        get
        {
            var mapper = new ButtonMapper<PSButton, XboxButton>();

            mapper.SetMapping(PSButton.DpadUp, XboxButton.DpadUp);
            mapper.SetMapping(PSButton.DpadDown, XboxButton.DpadDown);
            mapper.SetMapping(PSButton.DpadLeft, XboxButton.DpadLeft);
            mapper.SetMapping(PSButton.DpadRight, XboxButton.DpadRight);

            mapper.SetMapping(PSButton.Cross, XboxButton.A);
            mapper.SetMapping(PSButton.Circle, XboxButton.B);
            mapper.SetMapping(PSButton.Square, XboxButton.X);
            mapper.SetMapping(PSButton.Triangle, XboxButton.Y);

            mapper.SetMapping(PSButton.L1, XboxButton.LeftShoulder);
            mapper.SetMapping(PSButton.R1, XboxButton.RightShoulder);

            mapper.SetMapping(PSButton.L3, XboxButton.LeftStick);
            mapper.SetMapping(PSButton.R3, XboxButton.RightStick);

            mapper.SetMapping(PSButton.Share, XboxButton.Back);
            mapper.SetMapping(PSButton.Options, XboxButton.Start);
            mapper.SetMapping(PSButton.PS, XboxButton.Guide);

            return mapper;
        }
    }
}
