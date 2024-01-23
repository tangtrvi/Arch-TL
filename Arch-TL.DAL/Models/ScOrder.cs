using Arch_TL.DAL.Enums;

namespace Arch_TL.DAL.Models;

public sealed class ScOrder
{
    public string Key { get; }
    public OrderType Type { get; }

    public static implicit operator ScOrder((string key, OrderType type) order)
    {
        return Create(order.key, order.type);
    }

    private ScOrder(string key, OrderType type)
    {
        Key = key;
        Type = type;
    }

    public static ScOrder Create(string key, OrderType type)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        var instance = new ScOrder(key, type);
        return instance;
    }

    public string GetOrderString()
    {
        return string.Format("{0} {1}", Key, Type.ToString());
    }
}
