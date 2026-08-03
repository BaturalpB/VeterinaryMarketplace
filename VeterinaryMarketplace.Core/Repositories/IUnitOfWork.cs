namespace VeterinaryMarketplace.Core.Repositories
{
    public interface IUnitOfWork
    {
        Task CommitAsync(); 
        void Commit();  
    }
}