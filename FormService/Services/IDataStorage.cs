using FormService.Models;
namespace FormService.Services
{
    public interface IDataStorage
    {
        Task<bool> StoreAsync(string tableName, IEnumerable<BaseStorageRecord> records);
        Task<IEnumerable<BaseStorageRecord>> RetrieveAsync(string tableName, Guid id);
        Task<bool> DeleteAsync(string tableName, Guid id);
        Task<IEnumerable<BaseStorageRecord>> GetAllAsync(string tableName, int page = 1, int pageSize = 0);
        Task<IEnumerable<Guid>> SearchAsync(string tableName, StorageQuery query);
        Task<int> CountAsync(string tableName, StorageQuery? query = null);
    }
}
