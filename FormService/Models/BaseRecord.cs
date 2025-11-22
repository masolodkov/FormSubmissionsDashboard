namespace FormService.Models
{
    public record BaseRecord
    {
        public Guid SubmissionId { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Type { get; init; } = (int)TypeCode.Empty;
        public object? Value { get; init; }
    }
}