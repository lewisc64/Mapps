namespace Mapps.Gamepads.Events;

public interface IGamepadEventSource : IDisposable
{
    event EventHandler<IGamepadEventArgs>? EventDispatch;
}
