using GlobalDisasterManagement.Models;

namespace GlobalDisasterManagement.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<City> Cities { get; }
        IRepository<LGA> LGAs { get; }
        IRepository<User> Users { get; }
        IRepository<CsvFile> CsvFiles { get; }
        IRepository<CsvFileCity> CsvFileCities { get; }
        IRepository<CityFloodPrediction> CityPredictions { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
