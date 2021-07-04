using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TAP2018_19.AuctionSite.Interfaces;

namespace WU.Entity
{
    public class Site : ISite {
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

        string ISite.Name => Name;
        int ISite.Timezone => Timezone;
        int ISite.SessionExpirationInSeconds => SessionExpirationInSeconds;
        double ISite.MinimumBidIncrement => MinimumBidIncrement;

        void ISite.CleanupSessions() {
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                
            }

            throw new NotImplementedException();
        }

        void ISite.CreateUser(string username, string password) {
            throw new NotImplementedException();
        }

        void ISite.Delete() {
            throw new NotImplementedException();
        }

        IEnumerable<IAuction> ISite.GetAuctions(bool onlyNotEnded) {
            throw new NotImplementedException();
        }

        ISession ISite.GetSession(string sessionId) {
            throw new NotImplementedException();
        }

        IEnumerable<ISession> ISite.GetSessions() {
            throw new NotImplementedException();
        }

        IEnumerable<IUser> ISite.GetUsers() {
            throw new NotImplementedException();
        }

        ISession ISite.Login(string username, string password) {
            throw new NotImplementedException();
        }
    }
}