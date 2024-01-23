using System.Transactions;

namespace Arch_TL.DAL.Models;

public static class TransactionScopeFactory
{
    public static TransactionScope CreateReadCommitted()
    {
        return new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
            },
            TransactionScopeAsyncFlowOption.Enabled
        );
    }
}