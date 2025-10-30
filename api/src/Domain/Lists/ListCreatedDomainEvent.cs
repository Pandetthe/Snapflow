﻿using Snapflow.Common;

namespace Snapflow.Domain.Lists;

public sealed record ListCreatedDomainEvent(int BoardId, int SwimlaneId, string Title) : IDomainEvent<List>
{
    public int Id { get; private set; }

    public void SetEntity(List entity) => Id = entity.Id;
}
