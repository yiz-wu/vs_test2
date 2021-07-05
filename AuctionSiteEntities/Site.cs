using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;
using WU.Utilities;

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

        private bool IAmDeleted = false;
        private IAlarmClock alarmClock;
        string ISite.Name => Name;
        int ISite.Timezone => Timezone;
        int ISite.SessionExpirationInSeconds => SessionExpirationInSeconds;
        double ISite.MinimumBidIncrement => MinimumBidIncrement;

        void ISite.CleanupSessions()
        {
            if (IAmDeleted)
                throw new InvalidOperationException();
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
//                var NowTimeOfSite = DateTime.UtcNow.AddHours(Timezone);
                var NowTimeOfSite = alarmClock.Now;
                var expiredSessions = context.Sessions.Where(s=> s.SiteId == SiteId
                                                                && s.ValidUntil.CompareTo(NowTimeOfSite)<0);
                foreach (var session in expiredSessions)
                    context.Sessions.Remove(session);

                context.SaveChanges();
            }
        }

        void ISite.CreateUser(string username, string password) {
            UtilityMethods.CheckNullArgument(username, nameof(username));
            UtilityMethods.CheckNullArgument(password, nameof(password));
            UtilityMethods.CheckStringLength(username, nameof(username), DomainConstraints.MinUserName, DomainConstraints.MaxUserName);
            UtilityMethods.CheckStringLength(password, nameof(password), DomainConstraints.MinUserPassword, int.MaxValue);

            if (IAmDeleted)
                throw new InvalidOperationException();

            try {
                using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
                    var user = context.Users.Create();
                    user.Username = username;
                    user.Password = password;
                    user.SiteId = SiteId;

                    context.Users.Add(user);
                    context.SaveChanges();
                }
            } catch (Exception e) {
                if (e.InnerException != null && e.InnerException.InnerException != null) {
                    SqlException inner = (SqlException) e.InnerException.InnerException;
                    if (inner.Number == 2601)
                        throw new NameAlreadyInUseException(username, MethodBase.GetCurrentMethod().Name, e);
                }
                throw;
            }
            
        }

        void ISite.Delete() {
            if (IAmDeleted)
                throw new InvalidOperationException();
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                context.Sites.Attach(this);
                context.Sites.Remove(this);
                context.SaveChanges();
            }

            IAmDeleted = true;
        }

        IEnumerable<IAuction> ISite.GetAuctions(bool onlyNotEnded) {
            if (IAmDeleted)
                throw new InvalidOperationException();

            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                if (onlyNotEnded) {
//                    var NowTimeOfSite = DateTime.UtcNow.AddHours(Timezone);
                    var NowTimeOfSite = alarmClock.Now;
                    return context.Auctions.Where(a => a.SiteId == SiteId && a.EndsOn.CompareTo(NowTimeOfSite) > 0)
                        .ToList();
                }
                return context.Auctions.Where(a=> a.SiteId==SiteId).ToList();
            }
        }

        ISession ISite.GetSession(string sessionId) {
            UtilityMethods.CheckNullArgument(sessionId, nameof(sessionId));

            if (IAmDeleted)
                throw new InvalidOperationException();

            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
//                var NowTimeOfSite = DateTime.UtcNow.AddHours(Timezone);
                var NowTimeOfSite = alarmClock.Now;
                // looks for valid session on this site with that sessionId
                var session = context.Sessions.FirstOrDefault(s => s.SessionId == sessionId 
                                                                   && s.SiteId == SiteId
                                                                   && s.ValidUntil.CompareTo(NowTimeOfSite) >= 0);
                // default of user defined class is null
                return session;
            }
        }

        IEnumerable<ISession> ISite.GetSessions() {
            if (IAmDeleted)
                throw new InvalidOperationException();
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
                var sessions = context.Sessions.Where(s => s.SiteId == SiteId).ToList();
                return sessions;
            }
        }

        IEnumerable<IUser> ISite.GetUsers() {
            if (IAmDeleted)
                throw new InvalidOperationException();
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
                var users = context.Users.Where(u => u.SiteId == SiteId).ToList();
                return users;
            }
        }

        ISession ISite.Login(string username, string password) {
            UtilityMethods.CheckNullArgument(username, nameof(username));
            UtilityMethods.CheckNullArgument(password, nameof(password));
            UtilityMethods.CheckStringLength(username, nameof(username), DomainConstraints.MinUserName, DomainConstraints.MaxUserName);
            UtilityMethods.CheckStringLength(password, nameof(password), DomainConstraints.MinUserPassword, int.MaxValue);

            if (IAmDeleted)
                throw new InvalidOperationException();

            User user;
            Session session;
            // 1. check if pass for this user is correct
            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings))
            {
                var hashedPassword = UtilityMethods.GetHashString(password);
                user = context.Users.FirstOrDefault(u => u.Username == username
                                                && u.PasswordStored == hashedPassword);
                if (user == default)
                    return null;
            }
            

            using (var context = new AuctionSiteContext(AuctionSiteContext.ConnectionStrings)) {
                // 2. try to get user's session
                session = context.Sessions.FirstOrDefault(s=> s.UserId == user.UserId && s.SiteId == this.SiteId);

//                var NowTimeOfSite = DateTime.UtcNow.AddHours(Timezone);
                var NowTimeOfSite = alarmClock.Now;

                // if session not present create one and return it
                if (session == default) {
                    session = context.Sessions.Create();
                    session.SiteId = SiteId;
                    session.UserId = user.UserId;
                    session.SessionId = Session.sessionIdPool++.ToString();
                    session.ValidUntil = NowTimeOfSite.AddSeconds(SessionExpirationInSeconds);

                    context.Sessions.Add(session);
                    context.SaveChanges();
                    return session;
                }

                // if session is present but not valid, delete it and recreate one
                if (session.ValidUntil.CompareTo(NowTimeOfSite) < 0)
                {
                    var newSession = context.Sessions.Create();
                    newSession.SiteId = SiteId;
                    newSession.UserId = user.UserId;
                    newSession.SessionId = Session.sessionIdPool++.ToString();
                    newSession.ValidUntil = NowTimeOfSite.AddSeconds(SessionExpirationInSeconds);
                    newSession.SessionId = Session.sessionIdPool++.ToString();

                    context.Sessions.Add(newSession);
                    context.Sessions.Remove(session);
                    context.SaveChanges();
                    return newSession;
                }

                // reset ValidUntil time then return it
                session.ValidUntil = NowTimeOfSite.AddSeconds(SessionExpirationInSeconds);
                context.SaveChanges();
                return session;
            }

        }

        public void SetAlarmClock(IAlarmClock newAlarmClock) {
            this.alarmClock = newAlarmClock;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Site site = obj as Site;
            return this.Name.Equals(site.Name);
        }

        public override int GetHashCode() {
            return this.Name.GetHashCode();
        }
    }
}