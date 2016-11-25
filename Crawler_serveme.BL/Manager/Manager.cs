using Crawler_serveme.Core.Interfaces.Manager;

namespace Crawler_serveme.BL.Manager
{
    public class Manager : IManager
    {
        private readonly IBookingComManager _bookingComManager;

        public Manager(IBookingComManager bookingComManager)
        {
            _bookingComManager = bookingComManager;
        }

        public void GetInfoBookingComManager(string folder)
        {
            _bookingComManager.GetInfoBookingCom(folder);
        }
    }
}
