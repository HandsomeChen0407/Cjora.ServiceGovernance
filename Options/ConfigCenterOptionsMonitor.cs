using System.Text.Json;

namespace Cjora.ServiceGovernance.Options;

/// <summary>
/// 基于 IConfigCenter 的 OptionsMonitor
/// 支持配置热更新
/// </summary>
public sealed class ConfigCenterOptionsMonitor<T> :
    IOptionsMonitor<T> where T : class, new()
{
    private readonly IConfigCenter _configCenter;
    private readonly string _key;
    private readonly string _group;

    private T _currentValue;
    private readonly List<Action<T, string>> _listeners = new();

    public ConfigCenterOptionsMonitor(
        IConfigCenter configCenter,
        string key,
        string? group = null)
    {
        _configCenter = configCenter;
        _key = key;
        _group = group ?? "default";

        _currentValue = Load().GetAwaiter().GetResult();

        _configCenter.WatchConfigAsync(key, OnChanged, _group);
    }

    public T CurrentValue => _currentValue;

    public T Get(string? name) => _currentValue;

    public IDisposable OnChange(Action<T, string> listener)
    {
        _listeners.Add(listener);
        return new Handle(_listeners, listener);
    }

    private async Task<T> Load()
    {
        var json = await _configCenter.GetConfigAsync(_key, _group);
        return string.IsNullOrWhiteSpace(json)
            ? new T()
            : JsonSerializer.Deserialize<T>(json)!;
    }

    private void OnChanged(ConfigChangedEvent evt)
    {
        if (evt.IsDeleted)
            return;

        var value = JsonSerializer.Deserialize<T>(evt.Value!)!;
        _currentValue = value;

        foreach (var listener in _listeners)
            listener(value, evt.Key);
    }

    private sealed class Handle : IDisposable
    {
        private readonly List<Action<T, string>> _list;
        private readonly Action<T, string> _action;

        public Handle(List<Action<T, string>> list, Action<T, string> action)
        {
            _list = list;
            _action = action;
        }

        public void Dispose() => _list.Remove(_action);
    }
}
