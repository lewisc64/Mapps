using Mapps.Trackers;

namespace Mapps.Gamepads
{
    public interface IGamepad : IDisposable
    {
        bool IsConnected { get; }

        NumberTracker MeasuredPollingRate { get; }

        void Connect(string devicePath);

        void Disconnect();

        void TestRumble();
    }
}
