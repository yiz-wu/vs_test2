using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;

namespace WU.Entity {
    public class Session : ISession{
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

        string ISession.Id => SessionId;

        DateTime ISession.ValidUntil => ValidUntil;

        IUser ISession.User => OfUser;

        IAuction ISession.CreateAuction(string description, DateTime endsOn, double startingPrice) {
            throw new NotImplementedException();
        }

        bool ISession.IsValid() {
            throw new NotImplementedException();
        }

        void ISession.Logout() {
            throw new NotImplementedException();
        }
    }
}
