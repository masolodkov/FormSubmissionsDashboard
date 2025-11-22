using FormService.Models;

namespace FormService.Services
{
    public class DocumentFormProcessor : IFormProcessor
    {
        private readonly IDataStorage _storage;

        public DocumentFormProcessor(IDataStorage storage)
        {
            _storage = storage;
        }

        public async Task<FormSubmission?> ProcessAndStoreAsync(FormSubmissionDTO submission)
        {
            var fs = submission.MapToFormSubmission();
            var records = new List<BaseRecord>
            {
                new()
                {
                    SubmissionId = fs.Id,
                    Name = nameof(fs.SubmittedAt),
                    Type = (int)fs.SubmittedAt.GetTypeCode(),
                    Value = fs.SubmittedAt,
                },
                new()
                {
                    SubmissionId = fs.Id,
                    Name = nameof(fs.FormData),
                    Type = (int)TypeCode.String,
                    Value = fs.FormData,
                }
            };
            return await _storage.StoreAsync(submission.FormType, records)
                ? fs
                : null;
        }


        public async Task<FormSubmission?> GetByIdAsync(string formType, Guid id)
        {
            var records = await _storage.RetrieveAsync(formType, id);
            if (records == null)
            {
                return null;
            }
            var result = new FormSubmission()
            {
                FormType = formType,
                Id = id,
                SubmittedAt = (DateTime)records.First(r => r.Name == nameof(FormSubmission.SubmittedAt)).Value,
                FormData = (string)records.First(r => r.Name == nameof(FormSubmission.FormData)).Value
            };
            return result;
        }

        public async Task<IEnumerable<FormSubmission>> GetAllAsync(string formType)
        {
            var records = await _storage.GetAllAsync(formType);
            return records.GroupBy(r => r.SubmissionId)
                .Select(g => new FormSubmission()
                {
                    FormType = formType,
                    Id = g.Key,
                    SubmittedAt = (DateTime)g.First(r => r.Name == nameof(FormSubmission.SubmittedAt)).Value,
                    FormData = (string)g.First(r => r.Name == nameof(FormSubmission.FormData)).Value
                });
        }

        public Task<bool> DeleteAsync(string formType, Guid id)
        {
            return _storage.DeleteAsync(formType, id);
        }
    }
}
