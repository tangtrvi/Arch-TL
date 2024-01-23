using Arch_TL.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Extensions;

public static class QueryOrmExtentions
{
    public static UpdateBuilder<T> BuildUpdate<T>(this IQueryOrm orm) where T : class
    {
        return new UpdateBuilder<T>(orm);
    }
}