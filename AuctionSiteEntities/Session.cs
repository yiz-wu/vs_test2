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
    public class Session : ISession
    {
        // dato che il nostro AuctionSite ricrea DB ogni volta, posso inizializzarlo sempre a 0
        [NotMapped]
        public static int sessionIdPool = 0;


        [Key]
        public string SessionId { get; set; }
        [Required] 
        public int SiteId { get; set; }
        public virtual Site OfSite { get; set; }
        [Required]
        [Index(IsUnique = true)]
        public int UserId { get; set; }
        public virtual User OfUser { get; set; }
        [Required]
        public DateTime ValidUntil { get; set; }


        private bool isValid = true;
        public IAlarmClock AlarmClock;



        string ISession.Id => SessionId;

        DateTime ISession.ValidUntil => ValidUntil;

        IUser ISession.User {
            get
            {
                using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
                {
                    var user = context.Users.First(u => u.UserId == UserId);
                    return user;
                }
            }
        }

        IAuction ISession.CreateAuction(string description, DateTime endsOn, double startingPrice) {
            if (!isValid)
                throw new InvalidOperationException();

            UtilityMethods.CheckNullArgument(description, nameof(description));
            UtilityMethods.CheckStringLength(description, nameof(description), 0, int.MaxValue);
            UtilityMethods.CheckNumberOutOfRange(startingPrice, nameof(startingPrice), 0, double.MaxValue);


            ResetExpirationTime();

            Auction NewAuction;
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                NewAuction = context.Auctions.Create();
                NewAuction.SiteId = SiteId;
                NewAuction.SellerId = UserId;
                NewAuction.Description = description;
                NewAuction.CurrentPrice = startingPrice;
                NewAuction.EndsOn = endsOn;
                NewAuction.AlarmClock = AlarmClock;

                context.Auctions.Add(NewAuction);
                context.SaveChanges();
            }

            return NewAuction;
        }

        public void ResetExpirationTime()
        {
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                var me = context.Sessions.FirstOrDefault(s => s.SessionId == SessionId);
                var site = context.Sites.FirstOrDefault(s => s.SiteId == SiteId);
                // reset ValidUntil time
                var NowTimeOfSite = AlarmClock.Now.AddHours(site.Timezone);
                me.ValidUntil = NowTimeOfSite.AddSeconds(site.SessionExpirationInSeconds);

                context.SaveChanges();
            }
        }

        bool ISession.IsValid()
        {
            if (!isValid)
                return isValid;

            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
                var me = context.Sessions.FirstOrDefault(s => s.SessionId == SessionId);
                var site = context.Sites.FirstOrDefault(s => s.SiteId == SiteId);
                // if this session does not exist in DB, I do not know why this would happen
                if (me == default) {
                    isValid = false;
                    return isValid;
                }

                // if this session is still valid -> return true
                //                if (me.ValidUntil.CompareTo(DateTime.UtcNow.AddHours(site.Timezone)) > 0)
                if (me.ValidUntil.CompareTo(AlarmClock.Now) > 0)
                    return isValid;
                
                // otherwise delete itself
                isValid = false;
                context.Sessions.Remove(me);
                context.SaveChanges();
            }

            return isValid;
        }

        void ISession.Logout()
        {
            if (!isValid)
                throw new InvalidOperationException();

            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                var me = context.Sessions.First(s => s.SessionId == SessionId);
                context.Sessions.Remove(me);
                context.SaveChanges();
            }
            
            isValid = false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Session session = obj as Session;
            return this.SessionId.Equals(session.SessionId);
        }

        public override int GetHashCode() {
            return this.SessionId.GetHashCode();
        }
    }
}
