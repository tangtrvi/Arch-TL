﻿using Arch_TL.DAL.Context.Base;
using Arch_TL.DAL.Context.Entities;
using Arch_TL.DAL.Context.Interfaces;

namespace Arch_TL.DAL.Context.Repositories;

public class BiologyDomainRepository : Repository<BiologyDomain>, IBiologyDomainRepository
{
    public BiologyDomainRepository(IQueryOrm orm) : base(orm)
    {
    }
}
