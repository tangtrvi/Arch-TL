using Arch_TL.DAL.Models;

namespace Arch_TL.DAL.Context.Base;

public interface IDeleteRepository<T> where T : BaseEntity
{
    Task<T> GetVisibleByIdAsync(int id);
    Task<List<T>> GetVisibleByIdsAsync(List<int> id);
    Task<List<T>> GetVisibleListByColumnNameAsync(string columnName, object columnValue);
}