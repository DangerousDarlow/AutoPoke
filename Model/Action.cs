namespace Model;

public record Action
{
    public ActionType Type { get; init; }

    public int Amount { get; init; }
}

public enum ActionType
{
    Fold,
    Check,
    Call,
    Raise
}