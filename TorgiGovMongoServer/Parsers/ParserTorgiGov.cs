using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using TorgiGovMongoServer.BuilderApp;
using TorgiGovMongoServer.Logger;
using TorgiGovMongoServer.NetworkLibraries;

namespace TorgiGovMongoServer.Parsers
{
    public class ParserTorgiGov : ParserAbstract, IParser
    {
        private const string UrlPub =
            "http://torgi.gov.ru/opendata/7710349494-torgi/data-{bidKind}-{publishDateFrom}-{publishDateTo}-structure-20130401T0000.xml";

        private const string UrlChange =
            "http://torgi.gov.ru/opendata/7710349494-torgi/data.xml?bidKind={bidKind}&publishDateFrom={publishDateFrom}&publishDateTo={publishDateTo}&lastChangeFrom={lastChangeFrom}&lastChangeTo={lastChangeTo}";

        private IEnumerable<int> BitKinds => new[] {1};

        public void Parsing()
        {
            Parse(GeneratorUrl);
        }

        private void GeneratorUrl()
        {
            var nowDate = DateTime.Now;
            var currDate = $"{nowDate:yyyyMMdd}T0000";
            const string startDate = "20190101T0000";
            string parseUrl;
            switch (Builder.Arg)
            {
                case Arguments.Last:
                    parseUrl = UrlPub.Replace("{publishDateFrom}", startDate);
                    parseUrl = parseUrl.Replace("{publishDateTo}", currDate);
                    break;
                case Arguments.Curr:
                    var date3Days = $"{nowDate.AddDays(-3):yyyyMMdd}T0000";
                    parseUrl = UrlChange.Replace("{publishDateFrom}", date3Days);
                    parseUrl = parseUrl.Replace("{lastChangeFrom}", date3Days);
                    parseUrl = parseUrl.Replace("{publishDateTo}", currDate);
                    parseUrl = parseUrl.Replace("{lastChangeTo}", currDate);
                    ParserBidKind(parseUrl);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ParserBidKind(string url)
        {
            foreach (var b in BitKinds)
            {
                var urlB = url.Replace("{bidKind}", b.ToString());
                var xmlS = DownLoadString.DownloadString.DownL(urlB);
                if (string.IsNullOrEmpty(xmlS))
                {
                    Log.Logger("Получили пустую строку со списком торгов", urlB);
                    continue;
                }
                StringToJson(xmlS);
            }
        }

        private void StringToJson(string s)
        {
            var doc = new XmlDocument();
            doc.LoadXml(s);
            var jsons = JsonConvert.SerializeXmlNode(doc);
            using (var sw = new StreamWriter("torgi.json", false, System.Text.Encoding.Default))
            {
                sw.WriteLine(jsons);
            }
        }
    }
}