using Arch_TL.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Extensions
{
    public static class SqlBuilderExtensions<T> where T : class
    {
        public static Task<int> ExecutesAsync(List<UpdateBuilder<T>.ExecuteBuilder> builders)
        {
            if (builders == null || !builders.Any())
                return Task.FromResult(0);

            List<string> sqls = new List<string>();
            List<Dictionary<string, object>> parameters = new List<Dictionary<string, object>>();

            var builder = builders.FirstOrDefault().GetBuilder();

            foreach (var item in builders)
            {
                var sql = item.Sql();
                var param = item.GetBuilder().GetParameters();
                if (!string.IsNullOrWhiteSpace(sql) && param != null && param.Any())
                {
                    sqls.Add(sql);
                    parameters.Add(param);
                }
            }

            return builder.GetORM().ExecutesAsync(sqls, parameters);
        }
    }
}
