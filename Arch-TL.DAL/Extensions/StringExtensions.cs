using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Extensions;

public static class StringExtensions
{
    public static string FirstCharToUpper(this string input)
    {
        switch (input)
        {
            case null: return null;
            case "": return "";
            default: return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }
}