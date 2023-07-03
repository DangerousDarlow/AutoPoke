using System.Text.Json;
using Model;

namespace Tests;

public class EnvelopeTests
{
    [Test]
    public void Event_can_be_serialized_and_deserialized_using_envelope()
    {
        var eventIn = new TestEvent {Value = "Super!"};
        var envelopeIn = Envelope.CreateFromEvent(eventIn);

        var from = Guid.NewGuid();
        envelopeIn.Origin = from;

        var serialisedEnvelope = JsonSerializer.Serialize(envelopeIn);

        var envelopeOut = Envelope.CreateFromJson(serialisedEnvelope);
        Assert.That(envelopeOut.EventType, Is.EqualTo(typeof(TestEvent)));
        Assert.That(envelopeOut.Origin, Is.EqualTo(from));

        var eventOut = envelopeOut.ExtractEvent() as TestEvent;
        Assert.That(eventOut, Is.Not.Null);
        Assert.That(eventOut!.EventId, Is.EqualTo(eventIn.EventId));
        Assert.That(eventOut.Value, Is.EqualTo(eventIn.Value));
    }
}