using HidSharp;
using Mapps.Gamepads.DualShock4;
using Mapps.OutputWrappers;

void ConsoleGamepadReadout(DualShock4 gamepad)
{
    void WriteLine(string message)
    {
        var numSpaces = Console.BufferWidth - message.Length;
        while (numSpaces < 0)
        {
            numSpaces += Console.BufferWidth;
        }
        Console.WriteLine(message + new string(' ', numSpaces));
    }

    Console.CursorVisible = false;
    while (true)
    {
        Console.SetCursorPosition(0, 0);
        if (gamepad.IsConnected)
        {
            WriteLine(gamepad.IsBluetooth ? "Connection: bluetooth" : "Connection: cable");
        }
        else
        {
            WriteLine("Connection: disconnected");
        }
        WriteLine($"Battery level: {gamepad.Battery.Percentage}%" + (gamepad.Battery.IsCharging ? " (charging)" : string.Empty));
        WriteLine($"Average polling rate: {Math.Round(gamepad.MeasuredPollingRate.Average, 3)}ms");
        WriteLine("-");
        WriteLine($"Held buttons: {string.Join(", ", gamepad.Buttons.HeldButtons)}");
        WriteLine($"Left trigger: {gamepad.LeftTrigger.Pressure * 100}%");
        WriteLine($"Right trigger: {gamepad.RightTrigger.Pressure * 100}%");
        WriteLine($"Left stick: {gamepad.LeftJoystick.X}, {gamepad.LeftJoystick.Y}");
        WriteLine($"Right stick: {gamepad.RightJoystick.X}, {gamepad.RightJoystick.Y}");
        Thread.Sleep(30);
    }
}

HidDevice? SelectDevice()
{
    var devices = DualShock4.GetSupportedDevices();
    var wired = devices.FirstOrDefault(x => x.GetMaxInputReportLength() == 64);
    var bluetooth = devices.FirstOrDefault(x => x.GetMaxInputReportLength() > 64);
    return wired ?? bluetooth;
}

var connectedDevicePath = string.Empty;

using (var gamepad = new DualShock4())
using (var outputWrapper = new DualShock4ToXbox360(gamepad))
{
    new Thread(() => ConsoleGamepadReadout(gamepad)).Start();

    outputWrapper.Connect();

    while (true)
    {
        var desiredDevice = SelectDevice();
        if (desiredDevice != null && (!gamepad.IsConnected || connectedDevicePath != desiredDevice.DevicePath))
        {
            if (gamepad.IsConnected)
            {
                gamepad.Disconnect();
            }
            gamepad.Connect(desiredDevice.DevicePath);
            connectedDevicePath = desiredDevice.DevicePath;
        }

        Thread.Sleep(500);
    }
}
