using Force.Crc32;

namespace Mapps.Gamepads.DualShock4
{
    internal class HidOutputReport
    {
        private const int ReportLength = 334;

        public HidOutputReport()
        {
        }

        public bool EnableRumble { get; set; } = true;

        public bool EnableLightbar { get; set; } = true;

        public bool EnableFlash { get; set; } = true;

        public byte LeftHeavyMotor { get; set; } = 0;

        public byte RightLightMotor { get; set; } = 0;

        public byte LightBarRed { get; set; } = 0;

        public byte LightBarGreen { get; set; } = 0;

        public byte LightBarBlue { get; set; } = 0;

        public byte FlashOnDuration { get; set; } = 0;

        public byte FlashOffDuration { get; set; } = 0;

        public byte[] AsBytesUSB()
        {
            var bytes = new byte[ReportLength];

            bytes[0] = 0x05;
            WritePayload(bytes, 1);

            return bytes;
        }

        public byte[] AsBytesBluetooth(int pollRate)
        {
            var bytes = new byte[ReportLength];

            bytes[0] = 0x15;
            bytes[1] = (byte)(0xC0 | pollRate);
            bytes[2] = 0xA0;
            WritePayload(bytes, 3);

            var initial = Crc32Algorithm.Compute(new byte[] { 0xA2 });
            var crcValue = Crc32Algorithm.Append(initial, bytes, 0, bytes.Length - 4);

            bytes[bytes.Length - 4] = (byte)crcValue;
            bytes[bytes.Length - 3] = (byte)(crcValue >> 8);
            bytes[bytes.Length - 2] = (byte)(crcValue >> 16);
            bytes[bytes.Length - 1] = (byte)(crcValue >> 24);

            return bytes;
        }

        private void WritePayload(byte[] array, int offset)
        {
            array[0 + offset] = 0xF0;

            if (EnableRumble)
            {
                array[0 + offset] += 1;
            }
            if (EnableLightbar)
            {
                array[0 + offset] += 2;
            }
            if (EnableFlash)
            {
                array[0 + offset] += 4;
            }

            array[1 + offset] = 0x04;
            array[2 + offset] = 0;
            array[3 + offset] = RightLightMotor;
            array[4 + offset] = LeftHeavyMotor;
            array[5 + offset] = LightBarRed;
            array[6 + offset] = LightBarGreen;
            array[7 + offset] = LightBarBlue;
            array[8 + offset] = FlashOnDuration;
            array[9 + offset] = FlashOffDuration;
        }
    }
}
