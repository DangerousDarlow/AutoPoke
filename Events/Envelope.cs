using System.Text.Json;
using System.Text.Json.Serialization;

namespace Events;

public class Envelope
{
    [JsonConstructor]
    public Envelope(string eventTypeStr, string eventJson)
    {
        ArgumentException.ThrowIfNullOrEmpty(eventTypeStr);
        ArgumentException.ThrowIfNullOrEmpty(eventJson);
        EventTypeStr = eventTypeStr;
        EventJson = eventJson;

        var eventType = typeof(IEvent).Assembly.GetType(EventTypeStr);
        ArgumentNullException.ThrowIfNull(eventType);
        EventType = eventType;
    }

    public Envelope(Type eventType, string eventTypeStr, string eventJson)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentException.ThrowIfNullOrEmpty(eventTypeStr);
        ArgumentException.ThrowIfNullOrEmpty(eventJson);
        EventType = eventType;
        EventTypeStr = eventTypeStr;
        EventJson = eventJson;
    }

    [JsonIgnore] public Type EventType { get; }
    public string EventTypeStr { get; }
    public string EventJson { get; }

    public IEvent ExtractEvent()
    {
        var eventType = typeof(IEvent).Assembly.GetType(EventTypeStr);
        ArgumentNullException.ThrowIfNull(eventType);
        var @event = JsonSerializer.Deserialize(EventJson, eventType) as IEvent;
        ArgumentNullException.ThrowIfNull(@event);
        return @event;
    }

    public static Envelope CreateFromEvent<T>(T @event) where T : IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        var type = @event.GetType();
        var typeStr = type.FullName ?? type.Name;
        return new Envelope(type, typeStr, JsonSerializer.Serialize(@event));
    }

    public static Envelope CreateFromJson(string json)
    {
        var wrapper = JsonSerializer.Deserialize<Envelope>(json);
        ArgumentNullException.ThrowIfNull(wrapper);
        return wrapper;
    }
}