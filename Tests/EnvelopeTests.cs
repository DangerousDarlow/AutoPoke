using System.Text.Json;
using Events;

namespace Tests;

public class EnvelopeTests
{
    [Test]
    public void Event_can_be_serialized_and_deserialized_using_envelope()
    {
        var testIn = new TestEvent {Value = "Super!"};
        var wrapperIn = Envelope.CreateFromEvent(testIn);

        var json = JsonSerializer.Serialize(wrapperIn);

        var wrapperOut = Envelope.CreateFromJson(json);
        Assert.That(wrapperOut.EventType, Is.EqualTo(typeof(TestEvent)));

        var testOut = wrapperOut.ExtractEvent() as TestEvent;
        Assert.That(testOut, Is.Not.Null);
        Assert.That(testOut!.Value, Is.EqualTo(testIn.Value));
    }
}