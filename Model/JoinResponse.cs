namespace Model;

/// <summary>
/// Sent by the server to a client in response to a <see cref="JoinRequest"/>
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