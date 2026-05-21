using Microsoft.EntityFrameworkCore.Storage;

namespace TradeX.Repository.Abstractions.Models;

public sealed class TransactionModel(IDbContextTransaction? transaction) : IAsyncDisposable, IDisposable
{
    public IDbContextTransaction? Transaction { get; } = transaction;

    public void Dispose()
    {
        Transaction?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (Transaction is not null)
            await Transaction.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}
