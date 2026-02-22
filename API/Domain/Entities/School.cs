namespace Domain.Entities;

public class School
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string URN { get; set; } // Unique Reference Number (UK school identifier)
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string? Postcode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public SchoolType Type { get; set; }
    public int? Capacity { get; set; }
    public int? PupilsEnrolled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum SchoolType
{
    Primary = 1,
    Secondary = 2,
    Academy = 3,
    Independent = 4,
    Special = 5,
    Other = 6
}