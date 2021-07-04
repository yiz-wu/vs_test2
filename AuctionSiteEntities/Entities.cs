using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;

namespace WU.Entity
{
    public class AuctionSiteContext : DbContext {
        public DbSet<Site> Sites { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Auction> Auctions { get; set; }

        public AuctionSiteContext(string DbConnectionStrings) : base(DbConnectionStrings)
        {
            Database.SetInitializer<AuctionSiteContext>(new DropCreateDatabaseAlways<AuctionSiteContext>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
    public class Site {
        public int SiteId { get; set; }
        [Required]
        [Index(IsUnique = true)]
        [MinLength(DomainConstraints.MinSiteName)]
        [MaxLength(DomainConstraints.MaxSiteName)]
        public string Name { get; set; }
        [Required]
        [Range(DomainConstraints.MinTimeZone, DomainConstraints.MaxTimeZone)]
        public int Timezone { get; set; }
        [Required]
        [Range(Double.Epsilon, Double.PositiveInfinity)]
        public double MinimumBidIncrement { get; set; }
        [Required]
        [Range(0, Int32.MaxValue)]
        public int SessionExpirationInSeconds { get; set; }
    }
    public class User {
        public int UserId { get; set; }

        [Required]
        [Index("UsernameIsUniqueInEachSite", 1, IsUnique = true)]
        [MinLength(DomainConstraints.MinUserName)]
        [MaxLength(DomainConstraints.MaxUserName)]
        public string Username { get; set; }

        [Required]
        protected string PasswordStored { get; set; }
        [NotMapped]
        [MinLength(DomainConstraints.MinUserPassword)]
        public string Password {
            get { return PasswordStored; }
            set
            {
                var passAndSalt = value + UserId.ToString();
                using (var hashSystem = System.Security.Cryptography.SHA256.Create())
                {
                    PasswordStored = hashSystem.ComputeHash(Encoding.ASCII.GetBytes(passAndSalt)).ToString();
                }
            }
        }

        [Required]
        [Index("UsernameIsUniqueInEachSite", 2, IsUnique = true)]
        public virtual Site SiteId { get; set; }
    }
    public class Session {
        public int SessionId { get; set; }
        // [Required]
        // public virtual Site SiteId { get; set; }
        [Required]
        [Index(IsUnique = true)]
        public virtual User User { get; set; }
        [Required]
        public DateTime ValidUntil { get; set; }
    }
    public class Auction {
        public int AuctionId { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime EndsOn { get; set; }
        [Required]
        public virtual User Seller { get; set; }
        [Required]
        public double CurrentPrice { get; set; }
        public virtual User CurrentWinner { get; set; }
    }
}
