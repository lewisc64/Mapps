using System.Runtime.InteropServices;

namespace Mapps.Gamepads.Input.Xbox.GenericXInput;

public static class XInput
{
    private const string DllFile = "xinput1_4.dll";

    [StructLayout(LayoutKind.Explicit)]
    public struct XInputGamepad
    {
        [FieldOffset(0)]
        public ushort wButtons;

        [FieldOffset(2)]
        public byte bLeftTrigger;

        [FieldOffset(3)]
        public byte bRightTrigger;

        [FieldOffset(4)]
        public short sThumbLX;

        [FieldOffset(6)]
        public short sThumbLY;

        [FieldOffset(8)]
        public short sThumbRX;

        [FieldOffset(10)]
        public short sThumbRY;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct XInputState
    {
        [FieldOffset(0)]
        public uint dwPacketNumber;

        [FieldOffset(4)]
        public XInputGamepad Gamepad;
    }

    [DllImport(DllFile)]
    public static extern uint XInputGetState(uint dwUserIndex, ref XInputState pState);
}
