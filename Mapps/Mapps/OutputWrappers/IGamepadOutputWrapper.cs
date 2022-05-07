namespace Mapps.OutputWrappers
{
    public interface IGamepadOutputWrapper
    {
        bool IsConnected { get; }

        void Connect();

        void Disconnect();
    }
}
