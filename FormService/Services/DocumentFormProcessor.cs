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
            var records = new List<BaseStorageRecord>
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

        public async Task<PaginatedResult<FormSubmission>> GetAllAsync(string formType, int page, int pageSize)
        {
            var records = await _storage.GetAllAsync(formType, page, pageSize);
            var submissions = records.GroupBy(r => r.SubmissionId)
                .Select(g => new FormSubmission()
                {
                    FormType = formType,
                    Id = g.Key,
                    SubmittedAt = (DateTime)g.First(r => r.Name == nameof(FormSubmission.SubmittedAt)).Value,
                    FormData = (string)g.First(r => r.Name == nameof(FormSubmission.FormData)).Value
                });
            var totalCount = await _storage.CountAsync(formType);
            return new PaginatedResult<FormSubmission>
            {
                Items = submissions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        public Task<bool> DeleteAsync(string formType, Guid id)
        {
            return _storage.DeleteAsync(formType, id);
        }

        public async Task<PaginatedResult<FormSubmission>> SearchAsync(FormSearchRequest request)
        {
            if (string.IsNullOrEmpty(request.FormType))
            {
                return new PaginatedResult<FormSubmission>();
            }

            var storageQuery = ConvertToStorageQuery(request);

            var filteredIds = await _storage.SearchAsync(request.FormType, storageQuery);
            var totalCount = await _storage.CountAsync(request.FormType, storageQuery);

            var submissions = new List<FormSubmission>();
            foreach (var id in filteredIds)
            {
                var submission = await GetByIdAsync(request.FormType, id);
                if (submission != null)
                {
                    submissions.Add(submission);
                }
            }

            return new PaginatedResult<FormSubmission>
            {
                Items = submissions,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        private StorageQuery ConvertToStorageQuery(FormSearchRequest request)
        {
            var filters = new List<StorageFilter>();

            // Convert metadata filters
            if (request.FromDate.HasValue || request.ToDate.HasValue)
            {
                filters.Add(new StorageFilter
                {
                    JsonFieldName = string.Empty,
                    FieldPath = nameof(FormSubmission.SubmittedAt),
                    Operator = "range",
                    Value = request.FromDate ?? DateTime.MinValue,
                    Value2 = request.ToDate ?? DateTime.MaxValue
                });
            }

            // Convert JSON field filters
            foreach (var fieldFilter in request.FieldFilters)
            {
                filters.Add(new StorageFilter
                {
                    JsonFieldName = nameof(FormSubmission.FormData),
                    FieldPath = fieldFilter.FieldPath,
                    Operator = fieldFilter.Operator,
                    Value = fieldFilter.Value,
                    Value2 = fieldFilter.Value2
                });
            }

            return new StorageQuery
            {
                Filters = filters,
                Skip = (request.Page - 1) * request.PageSize,
                Take = request.PageSize
            };
        }
    }
}
