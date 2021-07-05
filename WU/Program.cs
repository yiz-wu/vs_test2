using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;
using WU.Entity;

namespace WU {
    class Program {

        private const string ConnectionString =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=FirstAuctionSiteDB;Integrated Security=True;";

        static void Main(string[] args)
        {

            Console.WriteLine("UTC :" +DateTime.UtcNow);
            Console.WriteLine("Now :" + DateTime.Now);
            Console.ReadLine();


            /*
            Console.WriteLine(AuctionSiteContext.ConnectionStrings);
            
            using (var context = new AuctionSiteContext(ConnectionString))
            {
                context.Database.Delete();
                context.Database.Create();
            }

            Console.WriteLine(AuctionSiteContext.ConnectionStrings);

            
            
            try
            {
                using (var context = new AuctionSiteContext(@"Data Source=pippo;Initial Catalog=pluto;Integrated Security=True;"))
                {
                    context.Sites.Select(s => s).ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught "+e.Message);
            }

            Console.WriteLine(AuctionSiteContext.ConnectionStrings);
            

            
            using (var context = new AuctionSiteContext(ConnectionString)) {
                var site = context.Sites.Create();
                site.Name = "First Site in DB v.2";
                site.MinimumBidIncrement = 123.4;
                site.SessionExpirationInSeconds = 10;
                site.Timezone = 0;

                context.Sites.Add(site);
                context.SaveChanges();
            }
            Console.WriteLine(AuctionSiteContext.ConnectionStrings);

            
            using (var context = new AuctionSiteContext(ConnectionString)) {
                var user = context.Users.Create();
                user.Username = "First User";
                user.Site = context.Sites.FirstOrDefault();
                user.Password = "password";

                context.Users.Add(user);
                context.SaveChanges();
            }
            using (var context = new AuctionSiteContext(ConnectionString)) {
                var user = context.Users.Create();
                user.Username = "Second User";
                user.Site = context.Sites.FirstOrDefault();
                user.Password = "password";

                context.Users.Add(user);
                context.SaveChanges();
            }
            using (var context = new AuctionSiteContext(ConnectionString))
            {
                foreach (var user in context.Users)
                {
                    Console.WriteLine(user.Username + " - " + user.Password);
                }
            
            }

            Console.WriteLine("end");
            Console.ReadLine();

            
            
            using (var context = new AuctionSiteContext(ConnectionString)) {
                var session = context.Sessions.Create();
                var user = context.Users.FirstOrDefault(p => p.UserId == 1);
                var site = context.Sites.FirstOrDefault(s => s.SiteId == user.SiteId.SiteId);
                session.User = user;
                session.ValidUntil = DateTime.Now.AddSeconds(site.SessionExpirationInSeconds);

                context.Sessions.Add(session);
                context.SaveChanges();
            }

            using (var context = new AuctionSiteContext(ConnectionString)) {
                var site = context.Sites.FirstOrDefault();

                context.Sites.Remove(site);
                context.SaveChanges();
            }*/

        }

    }
}
