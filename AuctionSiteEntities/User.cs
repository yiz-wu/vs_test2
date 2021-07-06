using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;
using WU.Utilities;

namespace WU.Entity {
    public class User : IUser{
        public int UserId { get; set; }

        [Required]
        [Index("UsernameIsUniqueInEachSite", 2, IsUnique = true)]
        [MinLength(DomainConstraints.MinUserName)]
        [MaxLength(DomainConstraints.MaxUserName)]
        public string Username { get; set; }

        [Required]
        [Index("UsernameIsUniqueInEachSite", 1, IsUnique = true)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }


        [Required]
        public virtual string PasswordStored { get; set; }
        [Required]
        [NotMapped]
        [MinLength(DomainConstraints.MinUserPassword)]
        public string Password {
            get { return PasswordStored; }
            set
            {
                // PasswordStored = UtilityMethods.EncryptPasswordGivenUsername(value, Username);
                PasswordStored = UtilityMethods.GetHashString(value);
            }
        }

        private bool IAmDeleted = false;

        string IUser.Username => Username;

        IEnumerable<IAuction> IUser.WonAuctions() {
            if (IAmDeleted)
                throw new InvalidOperationException();

            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                var site = context.Sites.FirstOrDefault(s=>s.SiteId == SiteId);
                var NowTimeOfSite = Site.AlarmClocks[SiteId].Now;

                var wonAuctions = context.Auctions.Where(a => a.CurrentWinnerId == UserId 
                                                              && a.EndsOn.CompareTo(NowTimeOfSite) < 0)
                    .ToList();
                return wonAuctions;
            }

        }

        void IUser.Delete() {
            if (IAmDeleted)
                throw new InvalidOperationException();
            // check if I have auctions not ended yet
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                var NowTimeOfSite = Site.AlarmClocks[SiteId].Now;
                var NotEndedAuction = context.Auctions.FirstOrDefault(a => a.SellerId == UserId
                                                                    && a.EndsOn.CompareTo(NowTimeOfSite) > 0);
                if(NotEndedAuction != default)
                    throw new InvalidOperationException();
            }


            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
                context.Users.Attach(this);
                context.Users.Remove(this);
                context.SaveChanges();
            }

            IAmDeleted = true;
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;

            User user = obj as User;


            return Username.Equals(user.Username) && SiteId.Equals(user.SiteId);
        }

        public override int GetHashCode() {
            return SiteId.GetHashCode() + Username.GetHashCode();
        }
    }
}
