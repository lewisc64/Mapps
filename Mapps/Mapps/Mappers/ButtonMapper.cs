namespace Mapps.Mappers;

public class ButtonMapper<TInput, TOutput>
    where TInput : notnull
    where TOutput : notnull
{
    private readonly Dictionary<TInput, TOutput> _mapping = new();

    public ButtonMapper()
    {
    }

    public IEnumerable<TInput> MappedButtons => _mapping.Keys;

    public void SetMapping(TInput input, TOutput output)
    {
        if (_mapping.ContainsKey(input))
        {
            _mapping[input] = output;
        }
        else
        {
            _mapping.Add(input, output);
        }
    }

    public void RemoveMapping(TInput button)
    {
        _mapping.Remove(button);
    }

    public bool HasMapping(TInput button)
    {
        return _mapping.ContainsKey(button);
    }

    public TOutput Map(TInput button)
    {
        return _mapping[button];
    }
}
