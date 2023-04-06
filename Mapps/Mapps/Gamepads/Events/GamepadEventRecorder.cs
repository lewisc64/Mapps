namespace Mapps.Gamepads.Events;

public class GamepadEventRecorder<TButton> : IGamepadEventProducer<TButton>
{
    private bool _disposed;
    private IGamepadEventProducer<TButton> _eventProducer;
    private EventHandler<IGamepadEventArgs>? _eventHandler;
    private List<(IGamepadEventArgs? Event, TimeSpan Stamp)> _recording = new();
    private DateTime? _recordingStartStamp;

    public event EventHandler<IGamepadEventArgs>? EventDispatch;

    public GamepadEventRecorder(IGamepadEventProducer<TButton> eventProducer)
    {
        _eventProducer = eventProducer;
    }

    public bool Recording { get; private set; } = false;

    public void StartRecording()
    {
        if (Recording)
        {
            throw new InvalidOperationException("Already recording.");
        }

        _recording.Clear();
        Recording = true;
        _recordingStartStamp = DateTime.UtcNow;

        _eventHandler = (_, gamepadEvent) =>
        {
            _recording.Add((gamepadEvent, DateTime.UtcNow - _recordingStartStamp.Value));
        };

        _eventProducer.EventDispatch += _eventHandler;
    }

    public void StopRecording()
    {
        if (!Recording)
        {
            throw new InvalidOperationException("Not current recording.");
        }

        _recording.Add((null, DateTime.UtcNow - _recordingStartStamp!.Value));

        Recording = false;
        _eventProducer.EventDispatch -= _eventHandler;
        _eventHandler = null;
    }

    public async Task PlayRecording(CancellationToken cancellationToken)
    {
        var startPlayingStamp = DateTime.UtcNow;

        foreach (var nextEvent in _recording)
        {
            var waitUntil = startPlayingStamp + nextEvent.Stamp;
            if (waitUntil - DateTime.UtcNow > TimeSpan.FromMilliseconds(1000))
            {
                try
                {
                    await Task.Delay(900, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // do nothing
                }
            }
            while (DateTime.UtcNow < waitUntil && !cancellationToken.IsCancellationRequested)
            {
                // do nothing, busy wait for accuracy
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (nextEvent.Event is not null)
            {
                EventDispatch?.Invoke(this, nextEvent.Event);
            }
        }
    }

    public void Register(IGamepad gamepad)
    {
        throw new NotSupportedException();
    }

    public void Deregister()
    {
        throw new NotSupportedException();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(GamepadEventRecorder<TButton>));
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (Recording)
                {
                    StopRecording();
                }
                _recording.Clear();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
