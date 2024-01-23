using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Models;

public sealed class DataPage<T>
{
    public static implicit operator DataPage<T>((List<T> Items, int Count, ScPagination Pagination) page)
    {
        return Create(page.Items, page.Count, page.Pagination);
    }

    public static implicit operator DataPage<T>((List<T> Items, int Count) page)
    {
        return Create(page.Items, page.Count);
    }

    public static DataPage<T> Create(List<T> items, int count)
    {
        return Create(items, count, null);
    }

    public static DataPage<T> CreateEmpty()
    {
        return (new List<T>(), 0, null);
    }

    public static DataPage<T> Create(List<T> items, int count, ScPagination pagination)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

        var instance = new DataPage<T>(items, count, pagination);
        return instance;
    }

    private DataPage(List<T> items, int count, ScPagination pagination)
    {
        Items = items;
        Count = count;

        Pagination = pagination;

        var pages = 1;
        if (pagination != null && pagination.PageNumber > 0 && pagination.PageSize > 0)
        {
            pages = Convert.ToInt32(Math.Ceiling((decimal)count / pagination.PageSize));
        }

        Pages = pages;

    }

    public int Pages { get; }
    public int Count { get; }
    public ScPagination Pagination { get; }
    public List<T> Items { get; }
}
