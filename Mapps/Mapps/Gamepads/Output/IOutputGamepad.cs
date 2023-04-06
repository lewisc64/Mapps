using Mapps.Gamepads.Events;
using Mapps.Gamepads.Input;
using Mapps.Mappers;

namespace Mapps.Gamepads.Output;

public interface IOutputGamepad<TDestinationButton> : IDisposable
    where TDestinationButton : notnull
{
    bool IsConnected { get; }

    void Connect();

    void Disconnect();

    void SetEventSource<TSourceButton>(IGamepadEventSource eventProducer, ButtonMapper<TSourceButton, TDestinationButton> buttonMapper)
        where TSourceButton : notnull;

    void DetachEventSource();

    void SetFeedbackGamepad(IInputGamepad? gamepad);
}
