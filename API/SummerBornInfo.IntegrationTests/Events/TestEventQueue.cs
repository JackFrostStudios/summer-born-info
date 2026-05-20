namespace SummerBornInfo.TestFramework.Events;

public sealed record TestEventQueue(string Name) : IEventQueue
{
    public string Name { get; private init; } = Name;
    public static TestEventQueue TestQueue => new(nameof(TestQueue));
}
