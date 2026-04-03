namespace SummerBornInfo.Infrastructure.Events;

public sealed record EventQueue(string Name) : IEventQueue
{
    public string Name { get; private init; } = Name;
    public static EventQueue SchoolBulkImport => new(nameof(SchoolBulkImport));
}
