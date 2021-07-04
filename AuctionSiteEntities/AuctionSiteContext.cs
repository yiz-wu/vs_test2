using System.Data.Entity;

namespace WU.Entity
{
    public class AuctionSiteContext : DbContext
    {
        public static string ConnectionStrings;
        public AuctionSiteContext(string DbConnectionStrings) : base(DbConnectionStrings)
        {
            ConnectionStrings = DbConnectionStrings;
        }

        public DbSet<Site> Sites { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Auction> Auctions { get; set; }


    }
}