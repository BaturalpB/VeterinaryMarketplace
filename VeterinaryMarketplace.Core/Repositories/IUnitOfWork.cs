namespace VeterinaryMarketplace.Core.Repositories
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task CommitAsync(); 
        void Commit();  
        
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}