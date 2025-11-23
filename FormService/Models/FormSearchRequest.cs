namespace FormService.Models
{
    public class FormSearchRequest
    {
        public string? FormType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<FormFieldFilter> FieldFilters { get; set; } = [];

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
