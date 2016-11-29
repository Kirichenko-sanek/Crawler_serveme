using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Crawler_serveme.Core.Interfaces.Manager;
using Crawler_serveme.Core.Interfaces.Repository;
using Crawler_serveme.Core.Model.Yelp;
using HtmlAgilityPack;

namespace Crawler_serveme.BL.Manager
{
    public class YelpComManager :IYelpComManager
    {
        public string UrlSite = "https://www.yelp.com";

        private readonly IRepository _repository;

        public YelpComManager(IRepository repository)
        {
            _repository = repository;
        }

        public void GetInfoYelpCom(string folder)
        {
            var city = GetCity();
            var places = GetPlaces("Restaurants", city);
        }

        public List<City> GetCity()
        {
            var cityList = new List<City>();
            
            var wClientStart = new WebClient();
            var htmlStart = new HtmlDocument();

            htmlStart.LoadHtml(wClientStart.DownloadString(UrlSite + "/locations"));

            var city = htmlStart.DocumentNode.SelectNodes("//ul[@class='cities']/li/a");
            Parallel.ForEach(city, item =>
            {
                var oneCity = new City
                {
                    CityName = item.InnerText,
                    Url = item.GetAttributeValue("href", "") ?? " "
                };
                cityList.Add(oneCity);
            });

            return cityList;
        }

        public List<Place> GetPlaces(string category, List<City> cityList)
        {
            var placeList = new List<Place>();
            Parallel.ForEach(cityList, city =>
            {
                var categoryList = GetAllCategory(city.CityName, category);
                if (categoryList.Count == 0)
                {
                    return;
                }
                Parallel.ForEach(categoryList, item =>
                {
                    var pages = GetPages(UrlSite + item);
                    if (pages.Count == 0)
                    {
                        return;
                    }
                    foreach (var page in pages)
                    {
                        var place = new Place
                        {
                            City = city.CityName,
                            Category = category,
                            Url = page
                        };
                        placeList.Add(place);
                    }
                });
            });
            //var allCategorys = GetAllCategory(city, category);
            //Parallel.ForEach(allCategorys, item =>
            //{
            //    using (var wClient = new WebClient())
            //    {
            //        var htmlSearch = new HtmlDocument();
            //        htmlSearch.LoadHtml(wClient.DownloadString("https://www.yelp.com/search?find_loc=" + city + "&cflt=" + item.InnerText));

            //    }
            //});


            return placeList;
        }

        public List<string> GetAllCategory(string city, string category)
        {
            
            var wClientStart = new WebClient();
            var htmlStart = new HtmlDocument();
            var result = new List<string>();
            try
            {
                htmlStart.LoadHtml(
                wClientStart.DownloadString(UrlSite + "/search?cflt=" + category + "&find_loc=" +
                                            city.Replace("/", "")));
                var allCategorys = htmlStart.DocumentNode.SelectNodes("//div[@class='all-category-browse-links']/ul/li/a");
                Parallel.ForEach(allCategorys, item =>
                {
                    try
                    {
                        result.Add(item.GetAttributeValue("href", ""));
                    }
                    catch (Exception)
                    {
                        return;
                    }

                });
            }
            catch (Exception)
            {
                result.Clear();
            }
            
            return result;
        }

        public int GetCoutPages(string url)
        {
            try
            {
                var wClientStart = new WebClient();
                var htmlStart = new HtmlDocument();
                htmlStart.LoadHtml(wClientStart.DownloadString(url));
                var cout =
                    Convert.ToInt32(
                        htmlStart.DocumentNode.SelectSingleNode(
                            "//div[@class='page-of-pages arrange_unit arrange_unit--fill']")
                            .InnerText.Replace("Page 1 of ", ""));
                return cout;
            }
            catch (Exception)
            {
                return -1;
            }        
        }

        public List<string> GetPages(string urlSearch)
        {
            var cout = GetCoutPages(urlSearch);
            var elementList = new List<string>();
            try
            {
                if (cout == -1)
                {
                    elementList.Clear();
                }
                else
                {
                    elementList.Clear();
                    Parallel.For(0, cout - 1, i =>
                    {
                        using (var wClient = new WebClient())
                        {
                            var html = new HtmlDocument();
                            try
                            {
                                html.LoadHtml(
                                    wClient.DownloadString(urlSearch + "start=" + (i*10)));
                            }
                            catch (Exception)
                            {
                                return;
                            }

                            var elements = html.DocumentNode.SelectNodes("//span[@class='indexed-biz-name']/a");
                            if (elements == null)
                            {
                                return;
                            }
                            foreach (var item in elements)
                            {
                                elementList.Add(UrlSite + item.GetAttributeValue("href", ""));
                            }
                        }
                    });
                }
            }
            catch (Exception)
            {
                elementList.Clear();
            }
            
            return elementList;
        }

    }
}
