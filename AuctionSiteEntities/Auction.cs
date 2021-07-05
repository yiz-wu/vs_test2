using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;
using WU.Utilities;

namespace WU.Entity {
    public class Auction : IAuction{
        public int AuctionId { get; set; }
        [Required]
        public string Description { get; set; }
        [Required] 
        public int SiteId { get; set; }
        public virtual Site OfSite { get; set; }
        [Required]
        public DateTime EndsOn { get; set; }

        [Required] 
        [ForeignKey("Seller")]
        public int SellerId { get; set; }
        public virtual User Seller { get; set; }

        [Required]
        public double CurrentPrice { get; set; }

        [ForeignKey("CurrentWinner")]
        public int? CurrentWinnerId { get; set; }
        public virtual User CurrentWinner { get; set; }


        private bool IAmDeleted = false;
        private bool FirstBid = true;
        private double MaximumOffer = 0;



        int IAuction.Id => AuctionId;

        IUser IAuction.Seller {
            get
            {
                using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
                {
                    var user = context.Users.First(u => u.UserId == SellerId);
                    return user;
                }
            }
        }

        string IAuction.Description => Description;

        DateTime IAuction.EndsOn => EndsOn;

        bool IAuction.BidOnAuction(ISession session, double offer) {
            if (IAmDeleted)
                throw new InvalidOperationException();
            UtilityMethods.CheckNumberOutOfRange(offer, nameof(offer), 0, double.MaxValue);
            UtilityMethods.CheckNullArgument(session, nameof(session));

            Session mySession;
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
                mySession = context.Sessions.First(s => s.SessionId == session.Id);
            }

            if (!session.IsValid())
                throw new ArgumentException("Session expired");
            if (mySession.UserId == SellerId)
                throw new ArgumentException("Logged user is the seller");
            if (mySession.SiteId != SiteId)
                throw new ArgumentException("Logged user is not user of this site");


            DateTime NowTimeOfSite;
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) 
            {
                var site = context.Sites.FirstOrDefault(s => s.SiteId == SiteId);
                NowTimeOfSite = DateTime.UtcNow.AddHours(site.Timezone);

                // auction already closed
                if(EndsOn.CompareTo(NowTimeOfSite) < 0)
                    throw new InvalidOperationException();

                mySession.ResetExpirationTime();

                // I cannot undestand the certain points of this auction system but whatever

                // 1st reject condition
                if (mySession.UserId == CurrentWinnerId && offer < MaximumOffer + site.MinimumBidIncrement)
                    return false;

                // 2nd reject condition
                if (mySession.UserId != CurrentWinnerId && offer <= CurrentPrice)
                    return false;

                // 3rd reject condition
                if (mySession.UserId != CurrentWinnerId && offer <= CurrentPrice + site.MinimumBidIncrement && !FirstBid)
                    return false;

                // 1st accept case
                if (FirstBid && offer >= CurrentPrice)
                {
                    FirstBid = false;
                    MaximumOffer = offer;
                    CurrentWinnerId = mySession.UserId;
                    return true;
                }

                // 2nd accept case
                if (mySession.UserId == CurrentWinnerId)
                {
                    MaximumOffer = offer;
                    return true;
                }

                // 3rd accept case
                if (!FirstBid && mySession.UserId != CurrentWinnerId && offer > MaximumOffer) {
                    CurrentPrice = Math.Min(MaximumOffer + site.MinimumBidIncrement, offer);
                    MaximumOffer = offer;
                    CurrentWinnerId = mySession.UserId;
                    return true;
                }

                // 4rd accept case
                if (!FirstBid && mySession.UserId != CurrentWinnerId && offer <= MaximumOffer)
                {
                    CurrentPrice = Math.Min(MaximumOffer, offer + site.MinimumBidIncrement);
                    return true;
                }
                
            }

            return false;

        }

        double IAuction.CurrentPrice() {
            if (IAmDeleted)
                throw new InvalidOperationException();
            return CurrentPrice;
        }

        IUser IAuction.CurrentWinner() {
            if (IAmDeleted)
                throw new InvalidOperationException();
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                var winner = context.Users.FirstOrDefault(u => u.UserId == CurrentWinnerId);
                return winner;
            }
        }

        void IAuction.Delete() {
            if (IAmDeleted)
                throw new InvalidOperationException();
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
                context.Auctions.Attach(this);
                context.Auctions.Remove(this);
                context.SaveChanges();
            }

            IAmDeleted = true;
        }


        public override bool Equals(object obj) {
            if (obj == null)
                return false;

            Auction auction = obj as Auction;
            return AuctionId.Equals(auction.AuctionId) && SiteId.Equals(auction.SiteId);
        }

        public override int GetHashCode() {
            return AuctionId.GetHashCode() + SiteId.GetHashCode();
        }
    }
}
