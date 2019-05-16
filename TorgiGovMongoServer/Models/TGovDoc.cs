using System;
using MongoDB.Bson;

namespace TorgiGovMongoServer.Models
{
    public class GovDoc
    {
        public ObjectId Id { get; set; }
        public string BidNumberG { get; set; }
        public DateTime LastChangedT { get; set; }
        public DateTime PublishDateT { get; set; }
        public string OdDetailedHrefT { get; set; }
        public int BidKindT { get; set; }
        public BsonDocument Dt { get; set; }
        public bool Send { get; set; }
    }
}