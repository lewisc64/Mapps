﻿using HidSharp;
using Mapps.Gamepads.Components;

namespace Mapps.Gamepads.DualShock4
{
    public class DualShock4 : HidGamepadBase, IHasButtons<DS4Button>, IHasDualJoysticks, IHasDualTriggers, IHasTwoDistinctMassRumbleMotors, IHasBattery
    {
        private const int VendorId = 0x054C;

        private static readonly int[] ProductIds = new[] { 0x05C4, 0x09CC };


        private const int SerialNumberFeatureId = 18;

        private const int UsbInputReportLength = 64;

        private readonly DS4HidOutputReport _outputReport = new DS4HidOutputReport();

        public DualShock4(string serialNumber)
        {
            SerialNumber = serialNumber;
        }

        public string SerialNumber { get; }

        public bool IsBluetooth => ActiveHidDevice?.GetMaxInputReportLength() > UsbInputReportLength;

        public Battery Battery { get; } = new Battery();

        public Buttons<DS4Button> Buttons { get; } = new Buttons<DS4Button>();

        public Joystick LeftJoystick { get; } = new Joystick();

        public Joystick RightJoystick { get; } = new Joystick();

        public Trigger LeftTrigger { get; } = new Trigger();

        public Trigger RightTrigger { get; } = new Trigger();

        public RumbleMotor HeavyMotor { get; } = new RumbleMotor();

        public RumbleMotor LightMotor { get; } = new RumbleMotor();

        public MutableRGBLight LightBar { get; } = new MutableRGBLight(0, 255, 255);

        public override async Task TestRumble()
        {
            ThrowIfDisposed();

            try
            {
                HeavyMotor.Intensity = 1;
                LightMotor.Intensity = 1;
                await Task.Delay(500);
            }
            finally
            {
                HeavyMotor.Intensity = 0;
                LightMotor.Intensity = 0;
            }
        }

        protected override IEnumerable<HidDevice> GetRelevantDevicesByPriority()
        {
            return DeviceList.Local.GetHidDevices()
                .Where(x => x.VendorID == VendorId && ProductIds.Contains(x.ProductID) && GetSerialNumber(x) == SerialNumber)
                .OrderBy(x => x.GetMaxInputReportLength());
        }

        protected override void ProcessInputReport(byte[] report)
        {
            ThrowIfDisposed();

            var parsedReport = new DS4HidInputReport(report, IsBluetooth);

            Battery.Percentage = parsedReport.BatteryPercentage;
            Battery.IsCharging = parsedReport.Charging;

            Buttons.HeldButtons = parsedReport.HeldButtons;

            LeftJoystick.X = ConvertJoystickValue(parsedReport.LeftJoystickX);
            LeftJoystick.Y = -ConvertJoystickValue(parsedReport.LeftJoystickY);
            RightJoystick.X = ConvertJoystickValue(parsedReport.RightJoystickX);
            RightJoystick.Y = -ConvertJoystickValue(parsedReport.RightJoystickY);

            LeftTrigger.Pressure = parsedReport.LeftTriggerPressure / 255f;
            RightTrigger.Pressure = parsedReport.RightTriggerPressure / 255f;
        }

        protected override byte[] GenerateOutputReport()
        {
            var heavyRumble = (byte)(HeavyMotor.Intensity * 255);
            var lightRumble = (byte)(LightMotor.Intensity * 255);

            if (_outputReport.LeftHeavyMotor != heavyRumble || _outputReport.RightLightMotor != lightRumble)
            {
                _outputReport.LeftHeavyMotor = heavyRumble;
                _outputReport.RightLightMotor = lightRumble;
                _outputReport.UpdateRumble = true;
            }

            if (_outputReport.LightBarRed != LightBar.Red || _outputReport.LightBarGreen != LightBar.Green || _outputReport.LightBarBlue != LightBar.Blue)
            {
                _outputReport.LightBarRed = LightBar.Red;
                _outputReport.LightBarGreen = LightBar.Green;
                _outputReport.LightBarBlue = LightBar.Blue;
                _outputReport.UpdateLightBar = true;
            }

            var bytes = IsBluetooth ? _outputReport.AsBytesBluetooth(OutputReportInterval.Milliseconds) : _outputReport.AsBytesUSB();

            _outputReport.UpdateRumble = false;
            _outputReport.UpdateLightBar = false;
            _outputReport.UpdateFlash = false;

            return bytes;
        }

        protected override void DisposeComponents()
        {
            Buttons.Dispose();
            LeftJoystick.Dispose();
            RightJoystick.Dispose();
            LeftTrigger.Dispose();
            RightTrigger.Dispose();
        }

        private static float ConvertJoystickValue(byte value)
        {
            return value / 255f * 2 - 1;
        }

        private static string? GetSerialNumber(HidDevice device)
        {
            if (device.GetMaxInputReportLength() == UsbInputReportLength)
            {
                var buffer = new byte[UsbInputReportLength];
                buffer[0] = SerialNumberFeatureId;

                using (var stream = device.Open())
                {
                    try
                    {
                        stream.GetFeature(buffer);
                    }
                    catch (IOException)
                    {
                        return null;
                    }
                }

                return Convert.ToHexString(buffer.Skip(1).Take(6).Reverse().ToArray()).ToLower();
            }
            else
            {
                return device.GetSerialNumber().ToLower();
            }
        }

        public static IEnumerable<string> GetSerialNumbers()
        {
            return DeviceList.Local.GetHidDevices()
                .Where(x => x.VendorID == VendorId && ProductIds.Contains(x.ProductID))
                .Select(x => GetSerialNumber(x))
                .Where(x => x != null)
                .Cast<string>()
                .Distinct();
        }
    }
}
