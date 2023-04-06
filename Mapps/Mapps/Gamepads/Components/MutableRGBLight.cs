namespace Mapps.Gamepads.Components;

public class MutableRGBLight : IGamepadComponent
{
    public MutableRGBLight()
    {
    }

    public MutableRGBLight(byte r, byte g, byte b)
    {
        Red = r;
        Green = g;
        Blue = b;
    }

    public byte Red { get; set; } = 255;

    public byte Green { get; set; } = 255;

    public byte Blue { get; set; } = 255;
}
