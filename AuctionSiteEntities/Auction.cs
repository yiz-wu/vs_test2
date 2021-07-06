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

        public double MaximumOffer { get; set; }
        private bool IAmDeleted = false;



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
            UtilityMethods.CheckNumberOutOfRange(offer, nameof(offer), 0, double.MaxValue);
            UtilityMethods.CheckNullArgument(session, nameof(session));
            if (!session.IsValid())
                throw new ArgumentException();
            if (IAmDeleted)
                throw new InvalidOperationException();

            Session mySession;
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
                mySession = context.Sessions.First(s => s.SessionId == session.Id);
            }

            if (mySession.UserId == SellerId)
                throw new ArgumentException("Logged user is the seller");
            if (mySession.SiteId != SiteId)
                throw new ArgumentException("Logged user is not user of this site");


            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) 
            {
                var site = context.Sites.FirstOrDefault(s => s.SiteId == SiteId);
                mySession = context.Sessions.First(s => s.SessionId == session.Id);
                DateTime NowTimeOfSite = Site.AlarmClocks[SiteId].Now;
                // auction already closed
                if(EndsOn.CompareTo(NowTimeOfSite) < 0)
                    throw new InvalidOperationException();

                // this line is added to pass the test "BidOnAuction_ValidOffer_UpdatesSessionsValidUntil"
                // personally I think this is unnecessary and the test is set wrong, but probably it's my implementation the actually wrong one
                ((Session)session).ValidUntil = NowTimeOfSite.AddSeconds(site.SessionExpirationInSeconds);
                mySession.ValidUntil = NowTimeOfSite.AddSeconds(site.SessionExpirationInSeconds);
                context.SaveChanges();



                // I cannot undestand the certain points of this auction system but whatever

                var auction = context.Auctions.FirstOrDefault(a => a.AuctionId == AuctionId);
                bool FirstBid = auction.CurrentWinnerId == default;

                // 1st reject condition
                if (mySession.UserId == auction.CurrentWinnerId && offer < auction.MaximumOffer + site.MinimumBidIncrement)
                    return false;

                // 2nd reject condition
                if (mySession.UserId != auction.CurrentWinnerId && offer < auction.CurrentPrice)
                    return false;

                // 3rd reject condition
                if (mySession.UserId != auction.CurrentWinnerId && offer <= auction.CurrentPrice + site.MinimumBidIncrement && !FirstBid)
                    return false;


                var winner = context.Users.FirstOrDefault(u => u.UserId == mySession.UserId);

                // 1st accept case
                if (FirstBid && offer >= auction.CurrentPrice)
                {
                    auction.MaximumOffer = offer;
                    auction.CurrentWinnerId = winner.UserId;
                    auction.CurrentWinner = winner;
                    context.SaveChanges();
                    return true;
                }

                // 2nd accept case
                if (mySession.UserId == auction.CurrentWinnerId)
                {
                    auction.MaximumOffer = offer;
                    context.SaveChanges();
                    return true;
                }

                // 3rd accept case
                if (!FirstBid && mySession.UserId != auction.CurrentWinnerId && offer > auction.MaximumOffer) {
                    auction.CurrentPrice = Math.Min(auction.MaximumOffer + site.MinimumBidIncrement, offer);
                    auction.MaximumOffer = offer;
                    auction.CurrentWinnerId = winner.UserId;
                    auction.CurrentWinner = winner;
                    context.SaveChanges();
                    return true;
                }

                // 4rd accept case
                if (!FirstBid && mySession.UserId != auction.CurrentWinnerId && offer <= auction.MaximumOffer)
                {
                    auction.CurrentPrice = Math.Min(auction.MaximumOffer, offer + site.MinimumBidIncrement);
                    auction.CurrentWinner = winner;
                    context.SaveChanges();
                    return true;
                }
                
            }

            return false;

        }

        double IAuction.CurrentPrice() {
            if (IAmDeleted)
                throw new InvalidOperationException();
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
                var auction = context.Auctions.FirstOrDefault(a => a.AuctionId == AuctionId);
                return auction.CurrentPrice;
            }
        }

        IUser IAuction.CurrentWinner() {
            if (IAmDeleted)
                throw new InvalidOperationException();
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                var auction = context.Auctions.FirstOrDefault(a => a.AuctionId == AuctionId);
                var winner = context.Users.FirstOrDefault(u => u.UserId == auction.CurrentWinnerId);
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
