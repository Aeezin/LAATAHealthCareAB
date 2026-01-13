using Microsoft.EntityFrameworkCore.Storage;

public sealed class FakeDbTransaction : IDbContextTransaction
{
    public Guid TransactionId { get; } = Guid.NewGuid();
    public bool Committed { get; private set; }
    public bool RolledBack { get; private set; }

    public void Commit() => Committed = true;
    public Task CommitAsync(CancellationToken cancellationToken = default) { Commit(); return Task.CompletedTask; }

    public void Rollback() => RolledBack = true;
    public Task RollbackAsync(CancellationToken cancellationToken = default) { Rollback(); return Task.CompletedTask; }

    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
