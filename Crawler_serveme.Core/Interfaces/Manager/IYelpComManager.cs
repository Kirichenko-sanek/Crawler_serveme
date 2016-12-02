using System.Collections.Generic;
using Crawler_serveme.Core.Model.Yelp;

namespace Crawler_serveme.Core.Interfaces.Manager
{
    public interface IYelpComManager
    {
        string GetProxy();
        void GetInfoYelpCom(string folder);
        List<City> GetCity();
        List<Place> GetPlaces(string category, City citys);
        List<string> GetAllCategory(string city, string category);
        List<string> GetPages(string urlSearch);
        List<Info> ParsePages(List<Place> places);
        Contact GetPlaceContact(string place);
        string HeaderOfFile();
        string ConvertInfoToString(List<Info> info);
    }
}
