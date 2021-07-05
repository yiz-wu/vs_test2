using System.Data.Entity;

namespace WU.Entity
{
    public class AuctionSiteContext : DbContext
    {
        public static string ConnectionStrings = "Unknown";
        private string AttemptedConnectionString = "Unknown";
        public AuctionSiteContext(string DbConnectionStrings) : base(DbConnectionStrings)
        {
            AttemptedConnectionString = DbConnectionStrings;
        }
        
        protected override void Dispose(bool disposing)
        {
            ConnectionStrings = AttemptedConnectionString;
            base.Dispose(disposing);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // delete "On Delete Cascade" for Session's FK to Site
            // unnecessary because deleting Site, all User will be deleted too,
            //                 and deleting Users, Session will be deleted too
            modelBuilder.Entity<Session>()
                .HasRequired(s => s.OfSite)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Auction>()
                .HasRequired(a => a.OfSite)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Auction>()
                .HasRequired(a => a.CurrentWinner)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Auction>()
                .HasOptional<User>(a => a.CurrentWinner)
                .WithOptionalPrincipal();
        }

        public DbSet<Site> Sites { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Auction> Auctions { get; set; }


    }
}