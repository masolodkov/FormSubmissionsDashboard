using System.Collections.Concurrent;
using FormService.Models;

namespace FormService.Services
{
    public class InMemoryStorage : IDataStorage
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, ConcurrentDictionary<string, BaseRecord>>> _tables = new();

        public Task<bool> StoreAsync(string tableName, IEnumerable<BaseRecord> records)
        {
            var submissionGuid = records.FirstOrDefault()?.SubmissionId;
            if (submissionGuid is null)
            {
                return Task.FromResult(false);
            }

            var table = _tables.GetOrAdd(tableName, _ => new ConcurrentDictionary<Guid, ConcurrentDictionary<string, BaseRecord>>());

            var formData = records.ToDictionary(r => r.Name, r => r);
            table[submissionGuid.Value] = new ConcurrentDictionary<string, BaseRecord>(formData);

            return Task.FromResult(true);
        }

        public Task<IEnumerable<BaseRecord>> RetrieveAsync(string tableName, Guid id)
        {
            if (_tables.TryGetValue(tableName, out var table) &&
                table.TryGetValue(id, out var records))
            {
                return Task.FromResult(records.Values.AsEnumerable());
            }

            return Task.FromResult(Enumerable.Empty<BaseRecord>());
        }

        public Task<IEnumerable<BaseRecord>> GetAllAsync(string tableName)
        {
            if (_tables.TryGetValue(tableName, out var table))
            {
                return Task.FromResult(table.Values.SelectMany(i => i.Values));
            }

            return Task.FromResult(Enumerable.Empty<BaseRecord>());
        }

        public Task<bool> DeleteAsync(string tableName, Guid id)
        {
            if (_tables.TryGetValue(tableName, out var table))
            {
                return Task.FromResult(table.TryRemove(id, out _));
            }

            return Task.FromResult(false);
        }
    }
}