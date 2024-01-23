using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Common;

internal static class StringHelper
{
    public static string ReplaceSingQuoteByDoubleQuote(this string value)
    {
        return value?.Replace("'", "''") ?? string.Empty;
    }

    public static string ReplaceSingQuoteByDoubleQuote(this object value)
    {
        return value.ToString().ReplaceSingQuoteByDoubleQuote();
    }
}
