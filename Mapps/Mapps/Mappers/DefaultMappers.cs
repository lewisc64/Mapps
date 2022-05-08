using Mapps.Gamepads.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Mapps.Mappers
{
    public static class DefaultMappers
    {
        public static ButtonMapper<DS4Button, Xbox360Button> DualShock4ToXbox360ButtonMapper
        {
            get
            {
                var mapper = new ButtonMapper<DS4Button, Xbox360Button>();

                mapper.SetMapping(DS4Button.DpadUp, Xbox360Button.Up);
                mapper.SetMapping(DS4Button.DpadDown, Xbox360Button.Down);
                mapper.SetMapping(DS4Button.DpadLeft, Xbox360Button.Left);
                mapper.SetMapping(DS4Button.DpadRight, Xbox360Button.Right);

                mapper.SetMapping(DS4Button.Cross, Xbox360Button.A);
                mapper.SetMapping(DS4Button.Circle, Xbox360Button.B);
                mapper.SetMapping(DS4Button.Square, Xbox360Button.X);
                mapper.SetMapping(DS4Button.Triangle, Xbox360Button.Y);

                mapper.SetMapping(DS4Button.L1, Xbox360Button.LeftShoulder);
                mapper.SetMapping(DS4Button.R1, Xbox360Button.RightShoulder);

                mapper.SetMapping(DS4Button.L3, Xbox360Button.LeftThumb);
                mapper.SetMapping(DS4Button.R3, Xbox360Button.RightThumb);

                mapper.SetMapping(DS4Button.Share, Xbox360Button.Back);
                mapper.SetMapping(DS4Button.Options, Xbox360Button.Start);
                mapper.SetMapping(DS4Button.PS, Xbox360Button.Guide);

                return mapper;
            }
        }
    }
}
