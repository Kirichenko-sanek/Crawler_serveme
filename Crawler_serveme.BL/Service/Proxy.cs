using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Crawler_serveme.Core.Interfaces.Service;
using HtmlAgilityPack;

namespace Crawler_serveme.BL.Service
{
    public class Proxy : IProxy
    {
        public string GetProxy()
        {
            
            var strings = new List<string>();
            //var newStrings = new List<string>();
            using (StreamReader sr = new StreamReader(@"../../../Crawler_serveme.BL/Resources/ProxyList.txt"))
            {
                while (true)
                {
                    string temp = sr.ReadLine();
                    if (temp == null) break;
                    strings.Add(temp);
                }
            }
            var newStrings = new List<string>(strings);
            var proxy = " ";
            foreach (var item in strings)
            {
                var wClientStart = new WebClient();
                wClientStart.Proxy = new WebProxy(item);
                var htmlStart = new HtmlDocument();
                try
                {
                    htmlStart.LoadHtml(wClientStart.DownloadString("https://www.yelp.com/locations"));
                    proxy = item;
                    break;
                }
                catch (Exception)
                {
                    newStrings.RemoveAt(0);
                }
   
            }
            File.WriteAllLines(@"../../../Crawler_serveme.BL/Resources/ProxyList.txt", newStrings);
            return proxy;
        }
    }
}
