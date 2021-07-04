using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Ninject.Modules;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;
using WU.Entity;
using WU.Utilities;

namespace WU.AuctionSite {
    class SiteFactory : ISiteFactory {
        void ISiteFactory.CreateSiteOnDb(string connectionString, string name, int timezone, int sessionExpirationTimeInSeconds, double minimumBidIncrement) {
            UtilityMethods.CheckNullArgument(connectionString, nameof(connectionString));
            UtilityMethods.CheckNullArgument(name, nameof(name));
            UtilityMethods.CheckStringLength(name, nameof(name), DomainConstraints.MinSiteName, DomainConstraints.MaxSiteName);
            UtilityMethods.CheckNumberOutOfRange(timezone, nameof(timezone), DomainConstraints.MinTimeZone, DomainConstraints.MaxTimeZone);
            UtilityMethods.CheckNumberOutOfRange(sessionExpirationTimeInSeconds, nameof(sessionExpirationTimeInSeconds), 0, double.PositiveInfinity);
            UtilityMethods.CheckNumberOutOfRange(minimumBidIncrement, nameof(minimumBidIncrement), 0, double.PositiveInfinity);

            try
            {
                using (var context = new AuctionSiteContext(connectionString))
                {
                    var site = context.Sites.Create();
                    site.Name = name;
                    site.SessionExpirationInSeconds = sessionExpirationTimeInSeconds;
                    site.MinimumBidIncrement = minimumBidIncrement;
                    site.Timezone = timezone;

                    context.Sites.Add(site);
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                SqlException inner = (SqlException) e.InnerException.InnerException;
                if (inner.Number == 2601)
                    throw new NameAlreadyInUseException(name, MethodBase.GetCurrentMethod().Name, e);

                throw new UnavailableDbException(MethodBase.GetCurrentMethod().Name, e);
            }

        }

        IEnumerable<string> ISiteFactory.GetSiteNames(string connectionString) {
            UtilityMethods.CheckNullArgument(connectionString, nameof(connectionString));

            try
            {
                using (var context = new AuctionSiteContext(connectionString))
                {
                    return context.Sites.Select(s => s.Name).ToList();
                }
            }
            catch (Exception e) {
                throw new UnavailableDbException(MethodBase.GetCurrentMethod().Name, e);
            }
        }

        int ISiteFactory.GetTheTimezoneOf(string connectionString, string name) {
            UtilityMethods.CheckNullArgument(connectionString, nameof(connectionString));
            UtilityMethods.CheckNullArgument(name, nameof(name));
            UtilityMethods.CheckStringLength(name, nameof(name), DomainConstraints.MinSiteName, DomainConstraints.MaxSiteName);

            try
            {
                ISite site;
                using (var context = new AuctionSiteContext(connectionString))
                {
                    site = context.Sites.First(s => s.Name == name);
                }
                
                return site.Timezone;
            }
            catch (InvalidOperationException e) {
                throw new InexistentNameException(name, MethodBase.GetCurrentMethod().Name,  e);
            }
            catch (Exception e) {
                throw new UnavailableDbException(MethodBase.GetCurrentMethod().Name, e);
            }
        }

        ISite ISiteFactory.LoadSite(string connectionString, string name, IAlarmClock alarmClock) {
            UtilityMethods.CheckNullArgument(connectionString, nameof(connectionString));
            UtilityMethods.CheckNullArgument(name, nameof(name));
            UtilityMethods.CheckNullArgument(alarmClock, nameof(alarmClock));
            UtilityMethods.CheckStringLength(name, nameof(name), DomainConstraints.MinSiteName, DomainConstraints.MaxSiteName);

            try
            {
                ISite site;
                using (var context = new AuctionSiteContext(connectionString))
                {
                    site = context.Sites.First(s => s.Name == name);
                }

                if (alarmClock.Timezone != site.Timezone)
                    throw new ArgumentException(nameof(alarmClock.Timezone));

                return site;
            }
            catch (InvalidOperationException e)
            {
                throw new InexistentNameException(name, MethodBase.GetCurrentMethod().Name, e);
            }
            catch (ArgumentException e) {
                throw;
            }
            catch (Exception e) {
                throw new UnavailableDbException(MethodBase.GetCurrentMethod().Name, e);
            }
        }
        
        void ISiteFactory.Setup(string connectionString) {
            UtilityMethods.CheckNullArgument(connectionString, nameof(connectionString));

            try {
                using (var context = new AuctionSiteContext(connectionString))
                {
                    context.Database.Delete();
                    context.Database.Create();
                }
            }
            catch (Exception e)
            {
                throw new UnavailableDbException(MethodBase.GetCurrentMethod().Name, e);
            }

        }
    }

    public class SiteFactoryModule : NinjectModule {
        public override void Load()
        {
            this.Bind<ISiteFactory>().To<SiteFactory>();
        }
    }
}
