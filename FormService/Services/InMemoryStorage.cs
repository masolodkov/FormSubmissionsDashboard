using System.Collections.Concurrent;
using System.Text.Json;
using FormService.Models;

namespace FormService.Services
{
    public class InMemoryStorage : IDataStorage
    {
        // FormType => SubmissionId => FieldValues
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, ConcurrentDictionary<string, BaseStorageRecord>>> _tables = new();

        public Task<bool> StoreAsync(string tableName, IEnumerable<BaseStorageRecord> records)
        {
            var submissionGuid = records.FirstOrDefault()?.SubmissionId;
            if (submissionGuid is null)
            {
                return Task.FromResult(false);
            }

            var table = _tables.GetOrAdd(tableName, _ => new ConcurrentDictionary<Guid, ConcurrentDictionary<string, BaseStorageRecord>>());

            var formData = records.ToDictionary(r => r.Name, r => r);
            table[submissionGuid.Value] = new ConcurrentDictionary<string, BaseStorageRecord>(formData);

            return Task.FromResult(true);
        }

        public Task<IEnumerable<BaseStorageRecord>> RetrieveAsync(string tableName, Guid id)
        {
            if (_tables.TryGetValue(tableName, out var table) &&
                table.TryGetValue(id, out var records))
            {
                return Task.FromResult(records.Values.AsEnumerable());
            }

            return Task.FromResult(Enumerable.Empty<BaseStorageRecord>());
        }

        public Task<IEnumerable<BaseStorageRecord>> GetAllAsync(string tableName, int page = 1, int pageSize = 0)
        {
            if (_tables.TryGetValue(tableName, out var table))
            {
                return pageSize > 0
                    ? Task.FromResult(table.Values
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .SelectMany(i => i.Values))
                    : Task.FromResult(table.Values
                        .SelectMany(i => i.Values));
            }

            return Task.FromResult(Enumerable.Empty<BaseStorageRecord>());
        }

        public Task<bool> DeleteAsync(string tableName, Guid id)
        {
            if (_tables.TryGetValue(tableName, out var table))
            {
                return Task.FromResult(table.TryRemove(id, out _));
            }

            return Task.FromResult(false);
        }

        public async Task<IEnumerable<Guid>> SearchAsync(string tableName, StorageQuery query)
        {
            if (!_tables.TryGetValue(tableName, out var table))
            {
                return [];
            }

            var results = table
                //.AsParallel() // may be parallel processing for larger datasets
                .Where(kvp => MatchesStorageQuery(kvp.Value, query.Filters))
                .Select(kvp => kvp.Key)
                .Skip(query.Skip)
                .Take(query.Take);

            return await Task.FromResult(results);
        }

        public async Task<int> CountAsync(string tableName, StorageQuery? query = null)
        {
            if (!_tables.TryGetValue(tableName, out var table))
            {
                return 0;
            }

            var count = query is null
                ? table.Keys.Count
                : table.Count(kvp => MatchesStorageQuery(kvp.Value, query.Filters));

            return await Task.FromResult(count);
        }

        private bool MatchesStorageQuery(ConcurrentDictionary<string, BaseStorageRecord> records, List<StorageFilter> filters)
        {
            foreach (var filter in filters)
            {
                if (!MatchesStorageFilter(records, filter))
                {
                    return false;
                }
            }
            return true;
        }

        private bool MatchesStorageFilter(ConcurrentDictionary<string, BaseStorageRecord> records, StorageFilter filter)
        {
            if (string.IsNullOrEmpty(filter.JsonFieldName))
            {
                if (records.TryGetValue(filter.FieldPath, out var record))
                {
                    if (record.Value is null)
                    {
                        return false;
                    }
                    return EvaluateFilter(record.Value, filter);
                }
                return false;
            }
            else
            {
                if (records.TryGetValue(filter.JsonFieldName, out var formDataRecord))
                {
                    if (formDataRecord.Value is null)
                    {
                        return false;
                    }
                    return EvaluateJsonFilter(formDataRecord.Value.ToString() ?? string.Empty, filter);
                }
                return false;
            }
        }

        private bool EvaluateFilter(object fieldValue, StorageFilter filter)
        {
            return filter.Operator.ToLower() switch
            {
                "equals" => fieldValue.Equals(filter.Value),
                "contains" => (fieldValue?.ToString() ?? string.Empty)
                    .Contains(filter.Value?.ToString() ?? string.Empty, StringComparison.InvariantCultureIgnoreCase),
                "range" when fieldValue is IComparable comparable
                          && (string.IsNullOrEmpty(filter.Value.ToString()) || comparable.CompareTo(filter.Value) >= 0)
                          && (string.IsNullOrEmpty(filter.Value2?.ToString()) || comparable.CompareTo(filter.Value2) <= 0) => true,
                _ => false
            };
        }

        private bool EvaluateJsonFilter(string formDataJson, StorageFilter filter)
        {
            try
            {
                var formData = JsonSerializer.Deserialize<Dictionary<string, object>>(formDataJson);
                if (formData == null || !formData.TryGetValue(filter.FieldPath, out object? fieldValue))
                {
                    return false;
                }

                return EvaluateFilter(fieldValue, filter);
            }
            catch
            {
                return false;
            }
        }
    }
}