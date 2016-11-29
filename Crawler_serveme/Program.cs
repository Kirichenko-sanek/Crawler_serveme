using System;
using Castle.Windsor;
using Crawler_serveme.BL.CW;
using Crawler_serveme.Controller;
using Crawler_serveme.Core.Interfaces.Manager;

namespace Crawler_serveme
{
    class Program
    {
        static void Main(string[] args)
        {
            var conteiner = new WindsorContainer().Install(new AdminInstaller());
            var contr = new Crawler_servemeController(conteiner.Resolve<IManager>());
            contr.StartCrawler();
            Console.ReadKey();
        }
    }
}
