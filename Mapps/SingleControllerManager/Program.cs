using Mapps.Gamepads.DualShock4;
using Mapps.OutputWrappers;

void ConsoleGamepadReadout(DualShock4 gamepad)
{
    void WriteLine(string message)
    {
        Console.WriteLine(message + new string(' ', Console.BufferWidth - message.Length % Console.BufferWidth));
    }

    while (true)
    {
        Console.CursorVisible = false;
        Console.SetCursorPosition(0, 0);
        if (gamepad.IsConnected)
        {
            WriteLine(gamepad.IsBluetooth ? "Connection: Bluetooth" : "Connection: USB");
        }
        else
        {
            WriteLine("Connection: Disconnected");
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

using (var gamepad = new DualShock4(DualShock4.GetSerialNumbers().First()))
using (var outputWrapper = new DualShock4ToXbox360(gamepad))
{
    new Thread(() => ConsoleGamepadReadout(gamepad)).Start();

    gamepad.StartTracking();
    outputWrapper.Connect();

    Console.ReadKey();
}
