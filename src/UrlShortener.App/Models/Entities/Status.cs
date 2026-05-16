namespace UrlShortener.App.Models.Entities;

public sealed class Status
{
    private Status() { }

    private readonly string[] ValidStatuses = ["Processing", "Active", "Inactive", "Invalid"];

    public Status(string value, string? description = null)
    {
        if (!ValidStatuses.Contains(value))
        {
            throw new ArgumentException($"Invalid status value: {value}. Valid values are: {string.Join(", ", ValidStatuses)}");
        }
        Value = value;
        Description = description;
    }

    public string Value { get; private set; } = string.Empty;

    public string? Description { get; private set; }
}