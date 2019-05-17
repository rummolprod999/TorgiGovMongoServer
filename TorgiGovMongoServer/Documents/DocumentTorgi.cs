using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using TorgiGovMongoServer.Logger;
using TorgiGovMongoServer.Models;
using TorgiGovMongoServer.NetworkLibraries;
using TorgiGovMongoServer.Parsers;

namespace TorgiGovMongoServer.Documents
{
    public class DocumentTorgi : AbstractDocument, IDocument
    {
        private readonly int _bidKind;
        private readonly int _isArchived;
        private readonly string _bidNumber;
        private readonly DateTime _lastChanged;
        private readonly DateTime _publishDate;
        private readonly string _odDetailedHref;
        private const string ColName = "torgigov";

        public DocumentTorgi(string bidNumber, DateTime lastChanged, DateTime publishDate, string odDetailedHref,
            int bidKind, int isArchived)
        {
            _bidNumber = bidNumber;
            _lastChanged = lastChanged;
            _publishDate = publishDate;
            _odDetailedHref = odDetailedHref;
            _bidKind = bidKind;
            _isArchived = isArchived;
        }

        public void ParsingDocument()
        {
            var db = ParserTorgiGov.Database;
            var col = db.GetCollection<GovDoc>(ColName);
            if (DelArchived(col)) return;
            var filter = new BsonDocument("$and",
                new BsonArray
                    {new BsonDocument("BidNumberG", _bidNumber), new BsonDocument("LastChangedT", _lastChanged)});
            var cursor = col.Find(filter);
            if (cursor.CountDocuments() != 0) return;
            
            FilterDoc(col).GetAwaiter().GetResult();

        }

        private bool DelArchived(IMongoCollection<GovDoc> col)
        {
            if (_isArchived != 1) return false;
            var filter = new BsonDocument("BidNumberG", _bidNumber);
            col.DeleteMany(filter);
            return true;

        }

        private async Task FilterDoc(IMongoCollection<GovDoc> col)
        {
            var filterUpdate = new BsonDocument("BidNumberG", _bidNumber);
            var docs = await col.Find(filterUpdate).ToListAsync();
            foreach (var govDoc in docs)
            {
                if (govDoc.LastChangedT >= _lastChanged) return;
            }

            var update = false;
            if (docs.Count > 0)
            {
                var resDel = await col.DeleteManyAsync(filterUpdate);
                update = resDel.DeletedCount > 0;
            }

            await InsertDoc(col, update);
        }

        private async Task InsertDoc(IMongoCollection<GovDoc> col, bool updated)
        {
            var xmlS = DownLoadString.DownloadString.DownL(_odDetailedHref);
            if (string.IsNullOrEmpty(xmlS))
            {
                Log.Logger("Получили пустую строку ", _odDetailedHref);
                return;
            }

            var doc = new XmlDocument();
            doc.LoadXml(xmlS);
            var jsons = JsonConvert.SerializeXmlNode(doc);
            var dt = BsonDocument.Parse(jsons);
            var m = dt.Elements.ToList()[1].Value.ToBsonDocument().Elements.ToList()[3].Value.ToBsonDocument();
            var tGov = new GovDoc
            {
                BidKindT = _bidKind, BidNumberG = _bidNumber, Dt = m, LastChangedT = _lastChanged,
                PublishDateT = _publishDate, OdDetailedHrefT = _odDetailedHref, Send = false
            };
            await col.InsertOneAsync(tGov);
            if (updated)
            {
                UpCount++;
            }
            else
            {
                Count++;
            }
        }
    }
}