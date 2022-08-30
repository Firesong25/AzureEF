
using Microsoft.EntityFrameworkCore;

namespace AzureEF
{
    public class LocalContext : DbContext
    {
        public DbSet<WowItem> WowItems { get; set; }
        public DbSet<WowAuction> WowAuctions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //commented for Martijn
            //string[] paths = { Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Data", "local_data.db" };
            //string fullPath = Path.Combine(paths);
            //optionsBuilder.UseSqlite($"Data Source={fullPath}");
            optionsBuilder.UseSqlite($"Data Source=local_data.db");
        }

    }

}
