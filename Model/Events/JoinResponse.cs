namespace Model.Events;

/// <summary>
/// Sent from the engine to a player in response to a <see cref="JoinRequest"/>
/// </summary>
public record JoinResponse : Event
{
    public JoinResponseStatus Status { get; init; }
}

public enum JoinResponseStatus
{
    Success,
    FailureEngineFull
}