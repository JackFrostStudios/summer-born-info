namespace SummerBornInfo.TestFramework.Events;

public sealed record TestEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
