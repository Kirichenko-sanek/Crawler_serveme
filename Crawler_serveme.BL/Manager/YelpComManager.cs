using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Crawler_serveme.Core.Interfaces.Manager;
using Crawler_serveme.Core.Interfaces.Repository;
using Crawler_serveme.Core.Model.Yelp;
using HtmlAgilityPack;

namespace Crawler_serveme.BL.Manager
{
    public class YelpComManager : IYelpComManager
    {
        public string UrlSite = "https://www.yelp.com";
        WebProxy myProxy = new WebProxy("31.193.129.158:80");

        private readonly IRepository _repository;

        public YelpComManager(IRepository repository)
        {
            _repository = repository;
        }

        public void GetInfoYelpCom(string folder)
        {
            var city = GetCity();
            var restaurants = GetPlaces("Nightlife", city);
            var restaurantsInfo = ParsePages(restaurants);

        }

        public List<City> GetCity()
        {
            var cityList = new List<City>();
            
            var wClientStart = new WebClient();
            var htmlStart = new HtmlDocument();
            
            wClientStart.Proxy = myProxy;
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
            wClientStart.Proxy = myProxy;
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
                wClientStart.Proxy = myProxy;
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
                            wClient.Proxy = myProxy;
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

        public List<Info> ParsePages(List<Place> places)
        {
            var infoList = new List<Info>();

            Parallel.ForEach(places, item =>
            {
                using (var wClient = new WebClient())
                {
                    var htmlInfo = new HtmlDocument();
                    wClient.Proxy = myProxy;
                    var information = new Info();
                    try
                    {
                        htmlInfo.LoadHtml(wClient.DownloadString(item.Url));
                    }
                    catch (Exception e)
                    {
                        return;
                    }

                    information.City = item.City;
                    information.Category = item.Category;
                    information.Url = item.Url;

                    information.Name =
                        htmlInfo.DocumentNode.SelectSingleNode("//div[@class='u-space-t1']/h1")?
                            .InnerText.Replace(",", ".") ?? " ";

                    var url =
                        htmlInfo.DocumentNode.SelectSingleNode("//span[@class='biz-website js-add-url-tagging']/a")?
                            .InnerText ?? " ";
                    if (url != " ")
                    {
                        information.PlaceUrl = "http://" + url;
                        var contact = GetPlaceContact("http://" + url);
                        information.Email = contact.Email;
                        var phone = contact.Tel;
                        information.Tel = phone == " "
                            ? (htmlInfo.DocumentNode.SelectSingleNode("//span[@class='biz-phone']")?.InnerText ?? " ")
                            : phone;
                    }
                    else
                    {
                        information.Tel =
                            htmlInfo.DocumentNode.SelectSingleNode("//span[@class='biz-phone']")?.InnerText ?? " ";
                        information.PlaceUrl = " ";
                    }
                    infoList.Add(information);

                }
            });



            return infoList;
        }

        public Contact GetPlaceContact(string place)
        {
            try
            {
                var result = new Contact();
                var emailsList = new List<string>();
                var telList = new List<string>();
                using (var wClientOrganizer = new WebClient())
                {
                    wClientOrganizer.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36");
                    wClientOrganizer.Headers.Add("Content-Type", "text/html, charset=utf-8");
                    wClientOrganizer.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    wClientOrganizer.Proxy = myProxy;
                    var htmlOrganizer = new HtmlDocument();
                    var regexEmail = new Regex(@"\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}\b");
                    var regexTel =
                        new Regex(
                            @"(^\+\d{1,2})?((\(\d{3}\))|(\-?\d{3}\-)|(\d{3}))((\d{3}\-\d{4})|(\d{3}\-\d\d\-\d\d)|(\d{7})|(\d{3}\-\d\-\d{3}))");

                    MatchCollection matchEmail;
                    MatchCollection matchTel;
                    try
                    {
                        htmlOrganizer.LoadHtml(wClientOrganizer.DownloadString(place + "/site/"));
                    }
                    catch (Exception e)
                    {
                        htmlOrganizer.LoadHtml(wClientOrganizer.DownloadString(place + "/"));
                    }
                    matchEmail = regexEmail.Matches(htmlOrganizer.DocumentNode.InnerText);
                    matchTel = regexTel.Matches(htmlOrganizer.DocumentNode.InnerText);
                    if (matchEmail.Count != 0)
                    {
                        foreach (Match item in matchEmail)
                        {
                            if (emailsList.Count != 0)
                            {
                                if (emailsList.Contains(item.Value))
                                {
                                    break;
                                }
                            }
                            emailsList.Add(item.Value);
                        }

                    }
                    if (matchTel.Count != 0)
                    {
                        foreach (Match item in matchTel)
                        {
                            if (telList.Count != 0)
                            {
                                if (telList.Contains(item.Value))
                                {
                                    break;
                                }
                            }
                            telList.Add(item.Value);
                        }
                    }

                    var elements = htmlOrganizer.DocumentNode.SelectNodes("//a[@href]");

                    if (elements != null)
                    {
                        var str = new List<string>();
                        foreach (var item in elements)
                        {
                            if (item.GetAttributeValue("href", "").Contains("contact"))
                            {
                                str.Add(item.GetAttributeValue("href", ""));
                            }
                            if (item.GetAttributeValue("href", "").Contains("about"))
                            {
                                str.Add(item.GetAttributeValue("href", ""));
                            }
                        }
                        if (str.Count != 0)
                        {
                            using (var wClientContact = new WebClient())
                            {
                                wClientContact.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36");
                                wClientContact.Headers.Add("Content-Type", "text/html, charset=utf-8");
                                wClientContact.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                                wClientContact.Proxy = myProxy;
                                var htmlContact = new HtmlDocument();
                                foreach (var cout in str)
                                {
                                    try
                                    {
                                        htmlContact.LoadHtml(wClientContact.DownloadString(cout));
                                    }
                                    catch (Exception)
                                    {
                                        try
                                        {
                                            htmlContact.LoadHtml(wClientContact.DownloadString(place + cout));
                                        }
                                        catch (Exception)
                                        {
                                            htmlContact.LoadHtml(wClientContact.DownloadString(place + "/" + cout));
                                        }

                                    }
                                    matchEmail = regexEmail.Matches(htmlContact.DocumentNode.InnerText);
                                    matchTel = regexTel.Matches(htmlContact.DocumentNode.InnerText);
                                    if (matchEmail.Count != 0)
                                    {
                                        foreach (Match item in matchEmail)
                                        {
                                            if (emailsList.Count != 0)
                                            {
                                                if (emailsList.Contains(item.Value))
                                                {
                                                    break;
                                                }
                                            }
                                            emailsList.Add(item.Value);
                                        }
                                    }
                                    if (matchTel.Count != 0)
                                    {
                                        foreach (Match item in matchTel)
                                        {
                                            if (telList.Count != 0)
                                            {
                                                if (telList.Contains(item.Value))
                                                {
                                                    break;
                                                }
                                            }
                                            telList.Add(item.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    result.Email = emailsList.Count != 0 ? string.Join("; ", emailsList.ToArray()) : " ";
                    result.Tel = telList.Count != 0 ? string.Join("; ", telList.ToArray()) : " ";
                    return result;
                }
            }
            catch (Exception e)
            {
                var result = new Contact
                {
                    Email = " ",
                    Tel = " "
                };
                return result;
            }
        }

    }
}
