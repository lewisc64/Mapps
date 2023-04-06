using Mapps.Gamepads.Events;

namespace Mapps.Gamepads.OutputWrappers;

public interface IGamepadOutputWrapper<TButton> : IDisposable
{
    bool IsConnected { get; }

    void Connect();

    void Disconnect();

    void SetEventSource(IGamepadEventProducer<TButton>? eventProducer);

    void SetFeedbackGamepad(IGamepad? gamepad);
}
