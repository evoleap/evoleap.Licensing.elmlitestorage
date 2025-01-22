namespace elmstoragerefimpl.Service.Interface
{
    public interface IStorageService
    {
        Task<IEnumerable<Guid>> ListObjectsAsync(string objectType);
        Task<object?> GetObjectAsync(string objectType, Guid id);
        Task<IEnumerable<object>> GetAllObjectsAsync(string objectType);
        Task<Guid> SaveObjectAsync(string objectType, Guid? id, string data);
        Task<bool> DeleteObjectAsync(string objectType, Guid id);
    }
}
