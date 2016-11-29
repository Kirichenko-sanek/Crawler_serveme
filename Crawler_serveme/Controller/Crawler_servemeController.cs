using Crawler_serveme.Core.Interfaces.Manager;

namespace Crawler_serveme.Controller
{
    public class Crawler_servemeController
    {
        private readonly IManager _manager;

        public Crawler_servemeController(IManager manager)
        {
            _manager = manager;
        }

        public void StartCrawler()
        {
            _manager.GetInfoYelpComManager("D:/");
            //_manager.GetInfoBookingComManager("");    
        }
    }
}
