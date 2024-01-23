using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Context.DbPartials;

public static partial class Db
{
    public static class Logging
    {
        public static class Columns
        {
            public const string Id = "id";
            public const string TimeStamp = "timestamp";
            public const string Level = "level";
            public const string Logger = "logger";
            public const string Callsite = "callsite";
            public const string Exception = "exception";
            public const string PostParams = "post_params";
        }
    }
}