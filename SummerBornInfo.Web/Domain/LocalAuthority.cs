﻿namespace SummerBornInfo.Web.Domain;

public class LocalAuthority
{
    public Guid Id { get; init; }
    public required int Code { get; init; }
    public required string Name { get; set; }
    public uint Version { get; init; }
}
