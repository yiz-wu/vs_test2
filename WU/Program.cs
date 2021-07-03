using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Ninject;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace WU {
    class Program {
        protected Mock<IAlarmClock> AlarmClock;


        static void Main(string[] args) {

            var kernel = new StandardKernel();
            
            ISiteFactory result = null;
            try {
                // kernel.Load(Configuration.ImplementationAssembly);
                // result = kernel.Get<ISiteFactory>();
            } catch (Exception e) {
                Console.WriteLine(e);
            }

        }
    }
}
