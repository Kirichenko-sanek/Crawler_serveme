using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Crawler_serveme.BL.Manager;
using Crawler_serveme.Core.Interfaces.Manager;
using Crawler_serveme.Core.Interfaces.Repository;

namespace Crawler_serveme.BL.CW
{
    public class AdminInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IRepository>().ImplementedBy<Repository.Repository>().LifestyleTransient());
            container.Register(Component.For<IManager>().ImplementedBy<Manager.Manager>().LifestyleTransient());
            container.Register(
                Component.For<IBookingComManager>().ImplementedBy<BookingComManager>().LifestyleTransient());
            container.Register(Component.For<IYelpComManager>().ImplementedBy<YelpComManager>().LifestyleTransient());
        }
    }
}
