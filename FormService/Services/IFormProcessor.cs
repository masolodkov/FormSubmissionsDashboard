using FormService.Models;

namespace FormService.Services
{
    public interface IFormProcessor
    {
        Task<bool> DeleteAsync(string formType, Guid id);
        Task<IEnumerable<FormSubmission>> GetAllAsync(string formType, int page, int pageSize);
        Task<FormSubmission?> GetByIdAsync(string formType, Guid id);
        Task<FormSubmission?> ProcessAndStoreAsync(FormSubmissionDTO submission);
        Task<PaginatedResult<FormSubmission>> SearchAsync(FormSearchRequest request);
    }
}