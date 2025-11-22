using FormService.Models;

namespace FormService.Services
{
    public interface IFormProcessor
    {
        Task<bool> DeleteAsync(string formType, Guid id);
        Task<IEnumerable<FormSubmission>> GetAllAsync(string formType);
        Task<FormSubmission?> GetByIdAsync(string formType, Guid id);
        Task<FormSubmission?> ProcessAndStoreAsync(FormSubmissionDTO submission);
    }
}