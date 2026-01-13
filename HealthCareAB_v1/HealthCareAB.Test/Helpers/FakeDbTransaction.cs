using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;


namespace HealthCareAB.Test.Helpers
{
    /// <summary>
    /// Simple fake IDbContextTransaction for unit tests.
    /// Tracks Commit/Rollback/Dispose so we can assert behavior.
    /// </summary>
    public sealed class FakeDbTransaction : IDbContextTransaction
    {
        public Guid TransactionId { get; } = Guid.NewGuid();

        public bool Committed { get; private set; }
        public bool RolledBack { get; private set; }
        public bool Disposed { get; private set; }

        public void Commit()
        {
            Committed = true;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            Commit();
            return Task.CompletedTask;
        }

        public void Rollback()
        {
            RolledBack = true;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            Rollback();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Disposed = true;
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}