using System.Text.Json;
using Events;
using NetMQ;
using NetMQ.Sockets;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

using var publisher = new PublisherSocket();
publisher.Bind("tcp://*:5556");

// Sleep to avoid ZeroMQ slow joiner syndrome where a send immediately after a bind is lost
Thread.Sleep(200);

var joinRequest = new JoinRequest {PlayerId = Guid.NewGuid(), PlayerName = "Client 1"};
var joinRequestEnvelope = Envelope.CreateFromEvent(joinRequest);

Log.Information("Sending {MessageType}", joinRequestEnvelope.EventTypeStr);
publisher.SendMoreFrame("Table").SendFrame(JsonSerializer.Serialize(joinRequestEnvelope));