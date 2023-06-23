using System.Text.Json;
using Events;

namespace Tests;

public class EnvelopeTests
{
    [Test]
    public void Event_can_be_serialized_and_deserialized_using_envelope()
    {
        var from = Guid.NewGuid();

        var eventIn = new TestEvent {Value = "Super!"};
        var envelopeIn = Envelope.CreateFromEvent(eventIn);
        envelopeIn.Origin = from;

        var serialisedEnvelope = JsonSerializer.Serialize(envelopeIn);

        var envelopeOut = Envelope.CreateFromJson(serialisedEnvelope);
        Assert.That(envelopeOut.EventType, Is.EqualTo(typeof(TestEvent)));
        Assert.That(envelopeOut.Origin, Is.EqualTo(from));

        var eventOut = envelopeOut.ExtractEvent() as TestEvent;
        Assert.That(eventOut, Is.Not.Null);
        Assert.That(eventOut!.Value, Is.EqualTo(eventIn.Value));
    }
}