using System.Data;

namespace Arch_TL.DAL.Context
{
    public interface IQueryOrm
    {
        Task<List<T>> QueryAsync<T>(string sql, object param = null, CommandType? commandType = null);

        Task<int> ExecuteAsync(string sql, object param = null);
        Task<int> ExecutesAsync(List<string> sqls, List<Dictionary<string, object>> parameters);
        Task<int> ExecuteScalarAsync(string sql, object param = null);

        Task<T> QueryFirstAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, CommandType? commandType = null);

        Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id");
        Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id");

        Task<int> InsertAsync<T>(T obj) where T : class;
        Task<int> InsertAsync<T>(List<T> entities) where T : class;

        Task<int> UpdateAsync<T>(int id, object obj, string keyName) where T : class;
        Task<int> UpdateAsync<T>(int id, T obj) where T : class;
    }
}
