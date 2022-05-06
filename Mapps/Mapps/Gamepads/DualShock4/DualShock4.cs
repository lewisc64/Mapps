using HidLibrary;
using Mapps.Gamepads.Components;

namespace Mapps.Gamepads.DualShock4
{
    public class DualShock4 : IGamepad
    {
        public const int VendorId = 0x054C;

        public static readonly int[] DeviceIds = new[] { 0x05C4, 0x09CC };

        private HidDevice _hidDevice;

        public DualShock4(HidDevice hidDevice)
        {
            _hidDevice = hidDevice;
            new Thread(() => { ProcessReports(); }).Start();
        }

        public Buttons<DS4Button> Buttons { get; } = new Buttons<DS4Button>();

        public Joystick LeftJoystick { get; } = new Joystick();

        public Joystick RightJoystick { get; } = new Joystick();

        public Trigger LeftTrigger { get; } = new Trigger();

        public Trigger RightTrigger { get; } = new Trigger();

        private void ProcessReports()
        {
            while (true)
            {
                var rawReport = _hidDevice.ReadReport();
                var payload = new HidReportPayload(rawReport.Data);

                Buttons.HeldButtons = payload.HeldButtons;

                LeftJoystick.X = payload.LeftJoystickX;
                LeftJoystick.Y = payload.LeftJoystickY;

                RightJoystick.X = payload.RightJoystickX;
                RightJoystick.Y = payload.RightJoystickY;

                LeftTrigger.Pressure = payload.LeftTriggerPressure;
                LeftTrigger.Active = Buttons.IsPressed(DS4Button.L2);

                RightTrigger.Pressure = payload.RightTriggerPressure;
                RightTrigger.Active = Buttons.IsPressed(DS4Button.R2);
            }
        }
    }
}
