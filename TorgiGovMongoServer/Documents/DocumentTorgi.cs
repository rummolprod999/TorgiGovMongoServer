using System;
using MongoDB.Bson;
using MongoDB.Driver;
using TorgiGovMongoServer.Parsers;

namespace TorgiGovMongoServer.Documents
{
    public class DocumentTorgi: AbstractDocument, IDocument
    {
        private int BidKind;
        private string bidNumber;
        private DateTime lastChanged;
        private DateTime publishDate;
        private string odDetailedHref;
        private const string ColName = "torgigov";
        
        public DocumentTorgi(string bidNumber, DateTime lastChanged, DateTime publishDate, string odDetailedHref, int bidKind)
        {
            this.bidNumber = bidNumber;
            this.lastChanged = lastChanged;
            this.publishDate = publishDate;
            this.odDetailedHref = odDetailedHref;
            BidKind = bidKind;
        }

        public void ParsingDocument()
        {
            var db = ParserTorgiGov.database;
            var col = db.GetCollection<BsonDocument>(ColName);
            var filter  = new BsonDocument("$and", new BsonArray {new BsonDocument("bidNumberG", bidNumber), new BsonDocument("lastChangedT", lastChanged)});
            var cursor =  col.Find(filter);
            if (cursor.CountDocuments() != 0)
            {
                Console.WriteLine("this document is exist");
                return;
            }


        }
    }
}