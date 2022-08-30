using AzureEF;
using Microsoft.EntityFrameworkCore;

namespace Cosmos.ModelBuilding;

public class AzureContext : DbContext
{
    public DbSet<WowAuction> WowAuctions { get; set; }
    public DbSet<WowItem> WowItems { get; set; }

    #region Configuration
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseCosmos(
            "AccountEndpoint=https://853786b9-0ee0-4-231-b9ee.documents.azure.com:443/;AccountKey=uH01pjKMd8RCx1BGv87XDOoeGvQjnh4h6FSjtV6mi5k4ZNx9CL0GtIf9Qup2uZnnHltMYnhuIbSsWuxKa1ygpw==",
            databaseName: "BlizzardData");
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region DefaultContainer
        modelBuilder.HasDefaultContainer("WowAuctions");
        #endregion

        #region Container
        modelBuilder.Entity<WowItem>()
            .ToContainer("WowItems");
        #endregion

        #region NoDiscriminator
        modelBuilder.Entity<WowAuction>()
            .HasNoDiscriminator();
        #endregion

        #region PartitionKey
        modelBuilder.Entity<WowAuction>().HasPartitionKey(o => o.PartitionKey);
        #endregion

        //#region ETag
        //modelBuilder.Entity<WowAuction>()
        //    .UseETagConcurrency();
        //#endregion

        #region PropertyNames
        //modelBuilder.Entity<WowAuction>().OwnsOne(l => l.ItemId);
        #endregion

        #region OwnsMany
        //modelBuilder.Entity<WowItem>().OwnsMany(p => p.ShippingCenters);
        //#endregion

        //#region ETagProperty
        //modelBuilder.Entity<WowItem>()
        //    .Property(d => d.ETag)
        //    .IsETagConcurrency();
        #endregion
    }
}