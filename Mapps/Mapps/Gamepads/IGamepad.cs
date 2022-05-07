namespace Mapps.Gamepads
{
    public interface IGamepad : IDisposable
    {
        bool IsConnected { get; }

        void Connect(string devicePath);

        void Disconnect();

        void TestRumble();
    }
}
