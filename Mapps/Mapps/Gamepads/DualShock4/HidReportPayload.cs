﻿namespace Mapps.Gamepads.DualShock4
{
    internal class HidReportPayload
    {
        private const double MaxBatteryValue = 20;

        private static readonly Dictionary<DS4Button, byte> MiscButtonMap = new Dictionary<DS4Button, byte>
        {
            { DS4Button.L1, 1 },
            { DS4Button.R1, 2 },
            { DS4Button.L2, 4 },
            { DS4Button.R2, 8 },
            { DS4Button.Share, 16 },
            { DS4Button.Options, 32 },
            { DS4Button.L3, 64 },
            { DS4Button.R3, 128 },
        };

        private static readonly Dictionary<DS4Button, byte> FaceButtonMap = new Dictionary<DS4Button, byte>
        {
            { DS4Button.Square, 16 },
            { DS4Button.Cross, 32 },
            { DS4Button.Circle, 64 },
            { DS4Button.Triangle, 128 },
        };

        private byte[] _raw;

        private int _offset;

        public HidReportPayload(byte[] raw, bool isBluetooth)
        {
            _offset = isBluetooth ? 3 : 1;
            _raw = raw;
        }

        private byte FaceButtonState => _raw[4 + _offset];

        private byte MiscButtonState => _raw[5 + _offset];

        private byte CounterByte => _raw[6 + _offset];

        public IEnumerable<DS4Button> HeldButtons
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
                    yield return DS4Button.DpadUp;
                }

                if (dpadState >= 1 && dpadState <= 3)
                {
                    yield return DS4Button.DpadRight;
                }

                if (dpadState >= 3 && dpadState <= 5)
                {
                    yield return DS4Button.DpadDown;
                }

                if (dpadState >= 5 && dpadState <= 7)
                {
                    yield return DS4Button.DpadLeft;
                }

                if ((CounterByte & 0x1) != 0)
                {
                    yield return DS4Button.PS;
                }

                if ((CounterByte & 0x2) != 0)
                {
                    yield return DS4Button.TouchPad;
                }
            }
        }

        public byte LeftJoystickX => _raw[0 + _offset];

        public byte LeftJoystickY => _raw[1 + _offset];

        public byte RightJoystickX => _raw[2 + _offset];

        public byte RightJoystickY => _raw[3 + _offset];

        public byte LeftTriggerPressure => _raw[7 + _offset];

        public byte RightTriggerPressure => _raw[8 + _offset];

        public byte BatteryLevel => _raw[11 + _offset];

        public double BatteryPercentage => Math.Min(BatteryLevel * 100 / MaxBatteryValue, 100);
    }
}
