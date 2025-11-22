namespace FormService.Models
{
    public class FormSubmissionDTO
    {
        public Guid? Id { get; set; }
        public string FormType { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }

        //any other submission-related data, eg:
        //public string SubmittedBy { get; set; } = string.Empty;
        //public Guid OwnerId {get; set;}
        //public string AccessLevel {get; set;}
        //public DateTime? UpdatedAt { get; set; }
        //public string Status { get; set; } = "submitted"; // submitted/processed/archived
        // public string FormVersion { get; set; } = "1.0"; 
        //...
        public string FormData { get; set; } = string.Empty;

        public FormSubmission MapToFormSubmission()
        {
            return new FormSubmission()
            {
                Id = this.Id ?? Guid.NewGuid(),
                SubmittedAt = this.SubmittedAt ?? DateTime.UtcNow,
                FormType = this.FormType ?? string.Empty,
                FormData = this.FormData
            };
        }

    }
}
