namespace Mapps.Gamepads.Events;

public class GamepadEventRecorder : IGamepadEventSource
{
    private bool _disposed;
    private IGamepadEventSource _eventSource;
    private EventHandler<IGamepadEventArgs>? _eventHandler;
    private List<(IGamepadEventArgs? Event, TimeSpan Stamp)> _recording = new();
    private DateTime? _recordingStartStamp;

    public event EventHandler<IGamepadEventArgs>? EventDispatch;

    public GamepadEventRecorder(IGamepadEventSource eventSource)
    {
        _eventSource = eventSource;
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

        _eventSource.EventDispatch += _eventHandler;
    }

    public void StopRecording()
    {
        if (!Recording)
        {
            throw new InvalidOperationException("Not currently recording.");
        }

        _recording.Add((null, DateTime.UtcNow - _recordingStartStamp!.Value));

        Recording = false;
        _eventSource.EventDispatch -= _eventHandler;
        _eventHandler = null;
    }

    public async Task PlayRecording(CancellationToken cancellationToken)
    {
        var startPlayingStamp = DateTime.UtcNow;

        foreach (var nextEvent in _recording)
        {
            var waitUntil = startPlayingStamp + nextEvent.Stamp;
            while (waitUntil - DateTime.UtcNow > TimeSpan.FromMilliseconds(1000) && !cancellationToken.IsCancellationRequested)
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

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(GamepadEventRecorder));
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
