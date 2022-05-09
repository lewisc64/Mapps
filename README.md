# Mapps
Controller mapper for the DualShock 4.

## Features/Limitations

* Switches between wired connections and bluetooth seamlessly (if both are connected, the wired connection will be preferred due to a faster polling rate).
* Gracefully handles disconnects/reconnects - the emulated Xbox360 controller doesn't get unplugged when connection to the DualShock 4 is lost.
* Each instance of the GUI maps one controller.
* Only official controllers are supported.
* There's currently no remapping or configuration profiles (planned).

## Requirements

* [.NET6.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* [ViGEmBus](https://github.com/ViGEm/ViGEmBus/releases/latest) - Used for emulating an Xbox360 controller
* An official Sony DualShock 4 controller
