namespace SummerBornInfo.Infrastructure.Events;

public sealed record EventQueue
{
    private EventQueue(string name)
    {
        Name = name;
    }
    public readonly string Name;
    public static EventQueue SchoolBulkImport => new(nameof(SchoolBulkImport));
    public static EventQueue TestQueue => new(nameof(TestQueue));
}
