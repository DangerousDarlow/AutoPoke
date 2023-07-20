namespace Model.Events;

/// <summary>
///     Player hole cards (not seen by other players). Sent by the engine directly to a player.
/// </summary>
public record HoleCards(Card Card1, Card Card2) : Event
{
    public override string ToString() => $"{Card1} {Card2}";
}