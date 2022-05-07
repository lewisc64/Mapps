using HidLibrary;
using Mapps.Gamepads.DualShock4;
using Mapps.OutputWrappers;

var devices = HidDevices.Enumerate(DualShock4.VendorId, DualShock4.ProductIds);

using (var gamepad = new DualShock4())
using (var outputWrapper = new DualShock4ToXbox360(gamepad))
{
    gamepad.Connect(devices.First().DevicePath);
    outputWrapper.Connect();
    Console.ReadLine();
}
