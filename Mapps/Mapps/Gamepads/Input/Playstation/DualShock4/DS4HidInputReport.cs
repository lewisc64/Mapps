namespace Mapps.Gamepads.Input.Playstation.DualShock4;

internal class DS4HidInputReport
{
    private static readonly Dictionary<PSButton, byte> MiscButtonMap = new Dictionary<PSButton, byte>
    {
        { PSButton.L1, 1 },
        { PSButton.R1, 2 },
        { PSButton.L2, 4 },
        { PSButton.R2, 8 },
        { PSButton.Share, 16 },
        { PSButton.Options, 32 },
        { PSButton.L3, 64 },
        { PSButton.R3, 128 },
    };

    private static readonly Dictionary<PSButton, byte> FaceButtonMap = new Dictionary<PSButton, byte>
    {
        { PSButton.Square, 16 },
        { PSButton.Cross, 32 },
        { PSButton.Circle, 64 },
        { PSButton.Triangle, 128 },
    };

    private readonly byte[] _raw;
    private readonly int _offset;

    public DS4HidInputReport(byte[] raw, bool isBluetooth)
    {
        _offset = isBluetooth ? 3 : 1;
        _raw = raw;
    }

    private byte FaceButtonState => _raw[4 + _offset];

    private byte MiscButtonState => _raw[5 + _offset];

    private byte CounterByte => _raw[6 + _offset];

    public IEnumerable<PSButton> HeldButtons
    {
        get
        {
            foreach (var (button, flag) in MiscButtonMap)
            {
                if ((MiscButtonState & flag) != 0)
                {
                    yield return button;
                }
            }

            foreach (var (button, flag) in FaceButtonMap)
            {
                if ((FaceButtonState & flag) != 0)
                {
                    yield return button;
                }
            }

            var dpadState = FaceButtonState & 0xF;

            if (dpadState <= 1 || dpadState == 7)
            {
                yield return PSButton.DpadUp;
            }

            if (dpadState >= 1 && dpadState <= 3)
            {
                yield return PSButton.DpadRight;
            }

            if (dpadState >= 3 && dpadState <= 5)
            {
                yield return PSButton.DpadDown;
            }

            if (dpadState >= 5 && dpadState <= 7)
            {
                yield return PSButton.DpadLeft;
            }

            if ((CounterByte & 0x1) != 0)
            {
                yield return PSButton.PS;
            }

            if ((CounterByte & 0x2) != 0)
            {
                yield return PSButton.TouchPad;
            }
        }
    }

    public byte LeftJoystickX => _raw[0 + _offset];

    public byte LeftJoystickY => _raw[1 + _offset];

    public byte RightJoystickX => _raw[2 + _offset];

    public byte RightJoystickY => _raw[3 + _offset];

    public byte LeftTriggerPressure => _raw[7 + _offset];

    public byte RightTriggerPressure => _raw[8 + _offset];

    public byte BatteryLevel => (byte)(_raw[29 + _offset] & 0x0F);

    public bool Charging => (_raw[29 + _offset] & 0x10) != 0;

    public int BatteryPercentage => Math.Min(BatteryLevel * 10 + 5, 100);
}
