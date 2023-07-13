using Microsoft.Extensions.Logging;
using Model;

namespace Logic.EngineEventHandlers;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class BeginHandHandler : IEngineEventHandler
{
    private readonly ILogger<BeginHandHandler> _logger;

    public BeginHandHandler(ILogger<BeginHandHandler> logger)
    {
        _logger = logger;
    }

    public IEngine Engine { get; set; } = null!;

    public Type TypeHandled => typeof(BeginHand);

    public OriginFilter OriginFilter => OriginFilter.EngineOnly;

    public void HandleEvent(IEvent @event)
    {
        Engine.Deck.Reset();

        foreach (var player in Engine.Players.Values)
        {
            Engine.SendToSingleClient(new HoleCards(Engine.Deck.Draw(), Engine.Deck.Draw()), player.Id);
            _logger.LogDebug("Hole cards dealt to player '{PlayerId}'", player.Id);
        }

        Engine.SendToAllClients(new HandStarted());
    }
}