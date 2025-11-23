namespace FormService.Models
{
    public record StorageQuery
    {
        public List<StorageFilter> Filters { get; set; } = [];
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
