using Arch_TL.DAL.Models;

namespace Arch_TL.DAL.Context.Base;

public interface IDisableAndDeleteRepository<T> : IDeleteRepository<T>, IDisableRepository<T> where T : BaseEntity
{
    Task<List<T>> GetAllVisibleAndEnabledAsync(ScPagination pagination);

    Task<T> GetVisibleAndDisabledByIdAsync(int id);
    Task<T> GetVisibleAndEnabledByIdAsync(int id);

    Task<List<T>> GetVisibleAndEnabledByIdsAsync(List<int> ids);
    Task<List<T>> GetVisibleAndEnabledListByColumnNameAsync(string columnName, object columnValue);
}
