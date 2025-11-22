using FormService.Models;
namespace FormService.Services
{
    public interface IDataStorage
    {
        Task<bool> StoreAsync(string tableName, IEnumerable<BaseRecord> records);
        Task<IEnumerable<BaseRecord>> RetrieveAsync(string tableName, Guid id);
        Task<bool> DeleteAsync(string tableName, Guid id);
        Task<IEnumerable<BaseRecord>> GetAllAsync(string tableName);
        //Task<IEnumerable<T>> SearchAsync(FormSearchRequest request);
    }
}
