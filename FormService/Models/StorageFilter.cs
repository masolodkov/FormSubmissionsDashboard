namespace FormService.Models
{
    public record StorageFilter
    {
        public string JsonFieldName { get; set; } = string.Empty;
        public string FieldPath { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public object? Value { get; set; }
        public object? Value2 { get; set; }
    }
}
