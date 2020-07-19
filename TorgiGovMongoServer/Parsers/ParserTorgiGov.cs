using System;
using System.Collections.Generic;
using System.Xml;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TorgiGovMongoServer.BuilderApp;
using TorgiGovMongoServer.Documents;
using TorgiGovMongoServer.Logger;
using TorgiGovMongoServer.NetworkLibraries;

namespace TorgiGovMongoServer.Parsers
{
    public class ParserTorgiGov : ParserAbstract, IParser
    {
        private const string UrlPub =
            "https://torgi.gov.ru/opendata/7710349494-torgi/data-{bidKind}-{publishDateFrom}-{publishDateTo}-structure-20130401T0000.xml";

        private const string UrlChange =
            "https://torgi.gov.ru/opendata/7710349494-torgi/data.xml?bidKind={bidKind}&publishDateFrom={publishDateFrom}&publishDateTo={publishDateTo}&lastChangeFrom={lastChangeFrom}&lastChangeTo={lastChangeTo}";

        public const string DbName = "torgi";
        public  static IMongoDatabase Database;
        private IEnumerable<int> BidKinds => new[] {13, 8, 1, 2};

        public void Parsing()
        {
            Parse(GeneratorUrl);
        }

        private void GeneratorUrl()
        {
            var nowDate = DateTime.Now;
            var currDate = $"{nowDate:yyyyMMdd}T{nowDate:HHmm}";
            const string startDate = "20190101T0000";
            string parseUrl;
            switch (Builder.Arg)
            {
                case Arguments.Last:
                    parseUrl = UrlPub.Replace("{publishDateFrom}", startDate);
                    parseUrl = parseUrl.Replace("{publishDateTo}", currDate);
                    break;
                case Arguments.Curr:
                    var date3Days = $"{nowDate.AddDays(-3):yyyyMMdd}T{nowDate.AddDays(-3):HHmm}";
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
            foreach (var b in BidKinds)
            {
                var urlB = url.Replace("{bidKind}", b.ToString());
                var xmlS = DownLoadString.DownloadString.DownL(urlB);
                if (string.IsNullOrEmpty(xmlS))
                {
                    Log.Logger("Получили пустую строку со списком торгов", urlB);
                    continue;
                }
                StringToJson(xmlS, b);
            }
        }

        private void StringToJson(string s, int bk)
        {
            var doc = new XmlDocument();
            doc.LoadXml(s);
            var jsons = JsonConvert.SerializeXmlNode(doc);
            /*using (var sw = new StreamWriter("torgi.json", false, System.Text.Encoding.Default))
            {
                sw.WriteLine(jsons);
            }*/
            var jo = JObject.Parse(jsons);
            CheckDocuments(jo, bk);
            
        }

        private void CheckDocuments(JObject o, int bk)
        {
            CreateDbIfNotExist(DbName);
            var cl = new MongoClient(Builder.ConnectString);
            Database = cl.GetDatabase(DbName);
            var notifications = GetElements(o, "openData.notification");
            foreach (var jToken in notifications)
            {
                try
                {
                    CheckDocument(jToken, bk);
                }
                catch (Exception e)
                {
                    Log.Logger(e);
                }
            }
        }

        private void CheckDocument(JToken token, int bk)
        {
            var isArchived = (int?) token.SelectToken("isArchived") ?? 0;
            if (isArchived > 0) return;
            var bidNumber = (string) token.SelectToken("bidNumber") ?? throw new Exception("bad bidNumber");
            var publishDate = (DateTime?) token.SelectToken("publishDate") ?? throw new Exception("bad publishDate");
            var lastChanged =  (DateTime?) token.SelectToken("lastChanged") ?? throw new Exception("bad lastChanged");
            var odDetailedHref = (string) token.SelectToken("odDetailedHref") ?? throw new Exception("bad odDetailedHref");
            var doc = new DocumentTorgi(bidNumber, lastChanged, publishDate, odDetailedHref, bk, isArchived);
            ParserDocument(doc);
        }
    }
}