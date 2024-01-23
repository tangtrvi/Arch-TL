using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Models;

public class SqlStatementBuilder : SqlBuilder
{
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