﻿namespace Events;

public record Card(Rank Rank, Suit Suit)
{
    public override string ToString() => $"{Rank}{Suit}";
}