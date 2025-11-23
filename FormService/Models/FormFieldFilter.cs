namespace FormService.Models
{
    public class FormFieldFilter
    {
        public string FieldPath { get; set; } = string.Empty; // "name", "country", "gender"
        public string Operator { get; set; } = string.Empty; // "equals", "contains", "range"
        public object? Value { get; set; }
        public object? Value2 { get; set; } // For range operations (date from/to)
    }
}
