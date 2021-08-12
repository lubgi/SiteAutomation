using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SiteAutomation
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlGenerator.GenerateRozetkaXml();
            XmlGenerator.GenerateGoogleFeedXml();
            PriceListProcessor.ProcessPriceList();
        }
    }
}