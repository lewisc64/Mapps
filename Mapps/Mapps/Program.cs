using HidLibrary;
using Mapps.Gamepads.DualShock4;

var devices = HidDevices.Enumerate(DualShock4.VendorId, DualShock4.DeviceIds);

var gamepad = new DualShock4(devices.First());
gamepad.StartTracking();

Console.ReadKey();
