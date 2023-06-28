using Events;
using ZeroMq;

namespace Logic;

public class Engine
{
    private readonly IServer _server;

    public Engine(IServer server)
    {
        _server = server;
        _server.ReceivedEvent += Handle;
    }

    private void Handle(Envelope envelope)
    {
        var @event = envelope.ExtractEvent();
        if (@event is not JoinRequest joinRequest) return;

        var joinResponse = new JoinResponse {Status = JoinResponseStatus.Success};
        _server.SendToSingleClient(Envelope.CreateFromEvent(joinResponse), joinRequest.PlayerId);
    }
}