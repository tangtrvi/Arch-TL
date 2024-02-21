using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using System.Transactions;

namespace Arch_TL.DAL.Context
{
    internal class QueryOrm : IQueryOrm
    {
        private readonly ILogger<QueryOrm> _logger;
        private readonly string _connectionStringRead;
        private readonly string _connectionStringWrite;
        private readonly int _statementTimeout;
        private readonly TimeSpan _defaultWarningLogDuration = TimeSpan.FromMilliseconds(3000);

        public QueryOrm(
            IConfiguration configuration,
            ILogger<QueryOrm> logger)
        {
            _logger = logger;
            _connectionStringRead = configuration["DBInfo:ConnectionString"];
            _connectionStringWrite = configuration["DBInfo:ConnectionStringWrite"];
            _statementTimeout = int.Parse(configuration["StatementTimeoutInSecond"]);
        }

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

        public async Task<List<T>> QueryAsync<T>(string sql, object param = null, CommandType? commandType = null)
        {
            using var connection = CreateReadConnection();
            var watch = Stopwatch.StartNew();

            try
            {
                await OpenConnectionAsync(connection);

                return (await connection.QueryAsync<T>(sql, param, commandType: commandType)).AsList();
            }
            catch (Exception ex)
            {
                LogError(ex, sql, param);
                return new List<T>();
            }
            finally
            {
                await connection.CloseAsync();
                EnsureLongRequestLogged(watch, sql, param);
            }
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

        private bool IsTransactionScopeExists()
        {
            return Transaction.Current != null;
        }

        private async Task OpenConnectionAsync(MySqlConnection connection)
        {
            await connection.OpenAsync();

            if (_statementTimeout == -1)
            {
                return;
            }

            var timeout = _statementTimeout;
            if (_statementTimeout == 0)
            {
                timeout = 10;
            }

            //var setStatementTimeoutForSessionOnly = $"SET statement_timeout = {TimeSpan.FromSeconds(timeout).TotalMilliseconds};";

            //await connection.ExecuteScalarAsync<int>(setStatementTimeoutForSessionOnly);
        }

        private MySqlConnection CreateReadConnection()
        {
            if (IsTransactionScopeExists())
            {
                return CreateWriteConnection();
            }
            return new MySqlConnection(_connectionStringRead);
        }

        private MySqlConnection CreateWriteConnection()
        {
            return new MySqlConnection(_connectionStringWrite);
        }

        private void EnsureLongRequestLogged(Stopwatch watch, string sql, object parameters = null, TimeSpan? warningLogDuration = null)
        {
            watch.Stop();

            if (watch.Elapsed < (warningLogDuration ?? _defaultWarningLogDuration)) return;

            //_logger.LogWarning(
            //    $"Long SQL execution duration: {watch.ElapsedMilliseconds} ms"
            //    + $"{Environment.NewLine}{sql}"
            //    + $"{Environment.NewLine}{parameters}"
            //);
        }

        private void LogError(Exception ex, string sql, object parameters = null)
        {
            //_logger.LogError(ex,
            //    $"Error while SQL execution: {ex}"
            //    + $"{Environment.NewLine}{sql}"
            //    + $"{Environment.NewLine}{parameters}"
            //);
        }

    }
}
