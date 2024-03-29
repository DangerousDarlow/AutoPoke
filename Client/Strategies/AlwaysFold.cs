﻿using Microsoft.Extensions.Logging;
using Model;
using Action = Model.Action;

namespace Client.Strategies;

// Class is used via reflection
// ReSharper disable once UnusedType.Global
public class AlwaysFold : IStrategy
{
    private readonly ILogger<AlwaysFold> _logger;

    public AlwaysFold(ILogger<AlwaysFold> logger)
    {
        _logger = logger;
    }

    public string Name => nameof(AlwaysFold);

    public IPlayer Player { get; set; } = null!;

    public Action Action() => new() {Type = ActionType.Fold};
}