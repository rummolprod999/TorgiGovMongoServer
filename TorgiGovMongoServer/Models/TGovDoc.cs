using System;
using MongoDB.Bson;

namespace TorgiGovMongoServer.Models
{
    public class TGovDoc
    {
        public ObjectId Id { get; set; }
        public string bidNumberG { get; set; }
        public DateTime lastChangedT { get; set; }
        public DateTime publishDateT { get; set; }
        public string odDetailedHrefT { get; set; }
        public int bidKindT { get; set; }
        public BsonDocument dT { get; set; }
    }
}