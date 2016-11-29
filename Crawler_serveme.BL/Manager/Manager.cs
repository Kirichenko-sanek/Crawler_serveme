using Crawler_serveme.Core.Interfaces.Manager;

namespace Crawler_serveme.BL.Manager
{
    public class Manager : IManager
    {
        private readonly IBookingComManager _bookingComManager;
        private readonly IYelpComManager _yelpComManager;

        public Manager(IBookingComManager bookingComManager, IYelpComManager yelpComManager)
        {
            _bookingComManager = bookingComManager;
            _yelpComManager = yelpComManager;
        }

        public void GetInfoBookingComManager(string folder)
        {
            _bookingComManager.GetInfoBookingCom(folder);
        }

        public void GetInfoYelpComManager(string folder)
        {
            _yelpComManager.GetInfoYelpCom(folder);
        }
    }
}
