using NewLagosFloodDetectionSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace NewLagosFloodDetectionSystem.Data
{
    public class FloodDbContext : IdentityDbContext<User>
    {
        public FloodDbContext(DbContextOptions<FloodDbContext> options) : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<LGA> LGAs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CsvFile> CsvFiles { get; set; }
        public DbSet<CsvFileCity> CsvFilesCities { get; set; }
        public DbSet<CityFloodPrediction> CityPredictions { get; set; }
    }
}