using System.IO;
using Crawler_serveme.Core.Interfaces.Repository;

namespace Crawler_serveme.BL.Repository
{
    public class Repository : IRepository
    {
        public void WriteToFile(string info, string fileName, string folder)
        {
            var str = folder + "/" + fileName.Replace("/", "_") + ".csv";
            using (StreamWriter theWriter = new StreamWriter(@"" + str, true))
            {
                theWriter.Write(info);
                theWriter.Close();
            }
        }
    }
}
