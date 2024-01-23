using Newtonsoft.Json;

namespace Arch_TL.DAL.Models;

[Serializable]
public class BaseEntity
{
    protected string GetEmailFormatedValue(string value)
    {
        return value?.ToLower().Trim() ?? string.Empty;
    }

    protected double GetValue(double value)
    {
        switch (value)
        {
            case double.NaN:
            case double.PositiveInfinity:
            case double.NegativeInfinity:
            case double.MinValue:
            case double.MaxValue:
                value = 0;
                break;
        }

        return value;
    }

    protected int? GetValue(int? value)
    {
        return value == 0 ? null : value;
    }

    protected object ParseFromObject(object value)
    {
        if (value == null)
        {
            return null;
        }

        if (value.GetType() == typeof(string) && !string.IsNullOrEmpty((string)value))
        {
            return JsonConvert.DeserializeObject((string)value);
        }

        return value;
    }
}
