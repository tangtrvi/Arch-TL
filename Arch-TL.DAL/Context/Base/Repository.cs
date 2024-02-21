using Arch_TL.DAL.Extensions;
using Arch_TL.DAL.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Context.Base;


public abstract class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IQueryOrm Orm;

    public Repository(IQueryOrm orm)
    {
        SimpleCRUD.SetDialect(SimpleCRUD.Dialect.MySQL);
        Orm = orm;
    }

    public UpdateBuilder<T> BuildUpdate()
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync(List<int> ids)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync(string key, object value)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetByKeyAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetByKeyAsync(string key, object value)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetByKeysAsync(List<int> ids)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetByKeysAsync(string key, List<object> values)
    {
        throw new NotImplementedException();
    }

    public string GetLimitString(int row, int page)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetListByColumnNameAsync(string columnName, object columnValue)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetListByColumnNameAsync(string columnName, List<object> values)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetPageAsync(ScPagination pagination, string searchText = null, params ScOrder[] orders)
    {
        var builder = new SqlStatementBuilder();
        var sql = builder.AddTemplate($"SELECT {Q<T>.Columns()} FROM {Q<T>.Table()} /**where**/ /**orderby**/ /**limit**/");

        builder.OnlyExisted<T>();
        builder.OnlyActive<T>();

        if (!string.IsNullOrEmpty(searchText))
            builder.SearchText<T>(searchText);

        builder.Limit(pagination);

        return Orm.QueryAsync<T>(sql.RawSql, sql.Parameters);
    }

    public Task<int> InsertAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertAsync(List<T> data)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpsertAsync<T2>(List<T2> entity) where T2 : class
    {
        throw new NotImplementedException();
    }
}

