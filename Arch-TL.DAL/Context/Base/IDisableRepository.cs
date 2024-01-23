using Arch_TL.DAL.Models;

namespace Arch_TL.DAL.Context.Base;

public interface IDisableRepository<T> where T : BaseEntity
{
    Task<List<T>> GetEnabledPageAsync(ScPagination pagination, params ScOrder[] orders);
    Task<T> GetEnabledByIdAsync(int id);
    Task<List<T>> GetEnabledByIdsAsync(List<int> id);
    Task<List<T>> GetEnabledListByColumnNameAsync(string columnName, object columnValue);
    Task<List<T>> GetEnabledListByColumnNameAsync(string columnName, List<object> values);
}