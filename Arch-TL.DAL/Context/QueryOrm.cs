using System.Data;

namespace Arch_TL.DAL.Context
{
    internal class QueryOrm : IQueryOrm
    {
        public Task<int> ExecuteAsync(string sql, object param = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecutesAsync(List<string> sqls, List<Dictionary<string, object>> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteScalarAsync(string sql, object param = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync<T>(T obj) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync<T>(List<T> entities) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> QueryAsync<T>(string sql, object param = null, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id")
        {
            throw new NotImplementedException();
        }

        public Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id")
        {
            throw new NotImplementedException();
        }

        public Task<T> QueryFirstAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, CommandType? commandType = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync<T>(int id, object obj, string keyName) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync<T>(int id, T obj) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
