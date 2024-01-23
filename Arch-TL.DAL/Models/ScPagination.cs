using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arch_TL.DAL.Models;

public sealed class ScPagination
{
    public static implicit operator ScPagination((int pageNumber, int pageSize) pagination)
    {
        return new ScPagination(pagination.pageNumber, pagination.pageSize);
    }

    private ScPagination(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageSize { get; }
    public int PageNumber { get; }

    public int GetLimit()
    {
        return PageSize;
    }

    public int GetOffset()
    {
        return (PageNumber - 1) * PageSize;
    }
}