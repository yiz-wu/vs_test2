using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;

namespace WU.Entity {
    public class Auction : IAuction{
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

        int IAuction.Id => AuctionId;

        IUser IAuction.Seller => Seller;

        string IAuction.Description => Description;

        DateTime IAuction.EndsOn => EndsOn;

        bool IAuction.BidOnAuction(ISession session, double offer) {
            throw new NotImplementedException();
        }

        double IAuction.CurrentPrice() {
            throw new NotImplementedException();
        }

        IUser IAuction.CurrentWinner() {
            throw new NotImplementedException();
        }

        void IAuction.Delete() {
            throw new NotImplementedException();
        }
    }
}
