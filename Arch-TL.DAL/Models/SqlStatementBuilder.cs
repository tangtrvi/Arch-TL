using Arch_TL.DAL.Context;
using Dapper;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Models;

public class SqlStatementBuilder : SqlBuilder
{
    public SqlBuilder AddCondition(string column, Type type, Dictionary<Type, string> values, string alias = null)
    {
        if (string.IsNullOrEmpty(column))
            return this;

        string resultAsText = values.GetValueOrDefault(type) ?? string.Empty;

        if (string.IsNullOrEmpty(resultAsText))
            return this;

        if (string.IsNullOrEmpty(alias))
            return Where(string.Format("{0} = {1}", column, resultAsText));

        return Where(string.Format("{0}.{1} = {2}", column, alias, resultAsText));
    }


    public SqlBuilder OnlyInactive<T>(string alias = null) where T : BaseEntity
    {
        return AddCondition(
            Q<T>.ActiveColumn(alias),
            Q<T>.ActiveColumnType(),
            new Dictionary<Type, string>
            {
                { typeof(int), "0" },
                { typeof(bool), "false" }
            },
            alias);
    }

    public SqlBuilder OnlyActive<T>(string alias = null) where T : BaseEntity
    {
        return AddCondition(
            Q<T>.ActiveColumn(alias), 
            Q<T>.ActiveColumnType(),
            new Dictionary<Type, string>
            {
                { typeof(int), "1" },
                { typeof(bool), "true" }
            },
            alias);
    }

    public SqlBuilder OnlyExisted<T>(string alias = null) where T : BaseEntity
    {
        return AddCondition(
            Q<T>.DeleteColumn(alias),
            Q<T>.DeleteColumnType(),
            new Dictionary<Type, string>
            {
                { typeof(int), "0" },
                { typeof(bool), "false" }
            },
            alias);
    }

    public SqlBuilder SearchText<T>(string searchText, string alias = null) where T : BaseEntity
    {
        var searchCollumns = Q<T>.SearchCollumns();
        if (searchCollumns.Count == 0)
            return this;

        if (!string.IsNullOrEmpty(alias))
            searchCollumns = searchCollumns.Select(x => string.Format("{0}.{1}", alias, x)).ToList();

        return Where(string.Format("UPPER(CONCAT_WS(' ', {0})) like @text", string.Join(",", searchCollumns)), new
        {
            text = string.Format("%{0}%", searchText.ToUpper())
        });
    }

    public SqlBuilder Limit(ScPagination pagination)
    {
        if (pagination == null)
        {
            return this;
        }

        if (pagination.PageNumber > 0 && pagination.PageSize > 0)
        {
            return AddClause("limit", "LIMIT @limit OFFSET @offset", new
            {
                limit = pagination.GetLimit(),
                offset = pagination.GetOffset()
            }, "\n ", null, "\n", false);
        }

        return this;
    }
}