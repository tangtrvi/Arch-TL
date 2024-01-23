using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Extensions;

public static class DataTypeExtensions
{
    public static bool IsNumeric(this Type tType)
    {
        return (tType.IsPrimitive && !(
               tType == typeof(bool)
            || tType == typeof(char)
            || tType == typeof(IntPtr)
            || tType == typeof(UIntPtr))) || (tType == typeof(decimal));
    }

    public static bool IsNullable(this Type tType)
    {
        return !tType.IsValueType || (Nullable.GetUnderlyingType(tType) != null);
    }

    public static TValue GetAttributeValue<TAttribute, TValue>(
    this Type type,
    Func<TAttribute, TValue> valueSelector)
    where TAttribute : Attribute
    {
        var att = type.GetCustomAttributes(
            typeof(TAttribute), true
        ).FirstOrDefault() as TAttribute;

        if (att != null)
        {
            return valueSelector(att);
        }

        return default(TValue);
    }
}