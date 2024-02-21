using Arch_TL.DAL.Models;

namespace Arch_TL.DAL.Context.Base;

public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByKeyAsync(int id);

    Task<T> GetByKeyAsync(string key, object value);

    Task<List<T>> GetByKeysAsync(List<int> ids);

    Task<List<T>> GetByKeysAsync(string key, List<object> values);

    Task<List<T>> GetPageAsync(ScPagination pagination, string searchText = null, params ScOrder[] orders);

    Task<int> InsertAsync(T entity);

    Task<int> InsertAsync(List<T> data);

    Task<int> DeleteAsync(int id);

    Task<int> DeleteAsync(List<int> ids);

    Task<int> DeleteAsync(string key, object value);

    Task<int> UpsertAsync<T2>(List<T2> entity) where T2 : class;

    Task<List<T>> GetListByColumnNameAsync(string columnName, object columnValue);

    Task<List<T>> GetListByColumnNameAsync(string columnName, List<object> values);

    string GetLimitString(int row, int page);

    UpdateBuilder<T> BuildUpdate();
}
