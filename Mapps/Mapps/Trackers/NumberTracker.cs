using System.Collections.Concurrent;

namespace Mapps.Trackers;

public class NumberTracker
{
    private int _maxSamples;

    private ConcurrentQueue<double> _samples = new ConcurrentQueue<double>();

    internal NumberTracker(int maxSamples)
    {
        _maxSamples = maxSamples;
    }

    public double Average => _samples.Any() ? _samples.Average() : 0;

    public double Max => _samples.Max();

    public double Min => _samples.Min();

    internal void AddSample(double sample)
    {
        _samples.Enqueue(sample);
        while (_samples.Count > _maxSamples)
        {
            _samples.TryDequeue(out double _);
        }
    }
}
