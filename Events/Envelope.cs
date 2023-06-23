using System.Text.Json;
using System.Text.Json.Serialization;

namespace Events;

public class Envelope
{
    [JsonConstructor]
    public Envelope(string eventTypeStr, string eventJson, Guid origin)
    {
        ArgumentException.ThrowIfNullOrEmpty(eventTypeStr);
        ArgumentException.ThrowIfNullOrEmpty(eventJson);
        ArgumentNullException.ThrowIfNull(eventJson);
        EventTypeStr = eventTypeStr;
        EventJson = eventJson;
        Origin = origin;

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
    public Guid Origin { get; set; }

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
        var envelope = new Envelope(type, typeStr, JsonSerializer.Serialize(@event));
        ArgumentNullException.ThrowIfNull(envelope);
        return envelope;
    }

    public static Envelope CreateFromJson(string json)
    {
        var envelope = JsonSerializer.Deserialize<Envelope>(json);
        ArgumentNullException.ThrowIfNull(envelope);
        return envelope;
    }
}