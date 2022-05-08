using Mapps.Trackers;

namespace Mapps.Gamepads
{
    public interface IGamepad : IDisposable
    {
        event EventHandler? OnConnect;

        event EventHandler? OnDisconnect;

        public event EventHandler? OnStateChanged;

        bool IsTracking { get; }

        bool IsConnected { get; }

        NumberTracker MeasuredPollingRate { get; }

        void StartTracking();

        void StopTracking();

        void TestRumble();
    }
}
