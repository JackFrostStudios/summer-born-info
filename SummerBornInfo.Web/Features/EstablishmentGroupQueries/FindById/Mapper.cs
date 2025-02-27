﻿namespace SummerBornInfo.Web.Features.EstablishmentGroupQueries.FindById;

internal sealed class Mapper : ResponseMapper<Response, EstablishmentGroup>
{
    public override Response FromEntity(EstablishmentGroup e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name
    };
}