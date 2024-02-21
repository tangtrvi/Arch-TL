using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Context.DbPartials;

public static partial class Db
{
    public static class BiologyDomain
    {
        public static class Columns
        {
            public const string Id = "id";
            public const string Name = "name";
            public const string Code = "code";
            public const string ImageUrl = "image_url";
            public const string Position = "position";
            public const string Description = "description";
        }
    }
}