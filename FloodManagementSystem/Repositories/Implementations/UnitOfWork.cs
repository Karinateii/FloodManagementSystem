using Microsoft.EntityFrameworkCore.Storage;
using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Repositories.Interfaces;

namespace GlobalDisasterManagement.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DisasterDbContext _context;
        private IDbContextTransaction? _transaction;
        
        public IRepository<City> Cities { get; private set; }
        public IRepository<LGA> LGAs { get; private set; }
        public IRepository<User> Users { get; private set; }
        public IRepository<CsvFile> CsvFiles { get; private set; }
        public IRepository<CsvFileCity> CsvFileCities { get; private set; }
        public IRepository<CityFloodPrediction> CityPredictions { get; private set; }

        public UnitOfWork(DisasterDbContext context)
        {
            _context = context;
            
            Cities = new Repository<City>(_context);
            LGAs = new Repository<LGA>(_context);
            Users = new Repository<User>(_context);
            CsvFiles = new Repository<CsvFile>(_context);
            CsvFileCities = new Repository<CsvFileCity>(_context);
            CityPredictions = new Repository<CityFloodPrediction>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
