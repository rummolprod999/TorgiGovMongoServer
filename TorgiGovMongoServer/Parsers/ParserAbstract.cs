using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using TorgiGovMongoServer.BuilderApp;
using TorgiGovMongoServer.Documents;
using TorgiGovMongoServer.Logger;

namespace TorgiGovMongoServer.Parsers
{
    public class ParserAbstract
    {
        protected void Parse(Action op)
        {
            Log.Logger("Время начала парсинга");
            op?.Invoke();
            Log.Logger("Добавили Tender", AbstractDocument.Count);
            Log.Logger("Обновили Tender", AbstractDocument.UpCount);
            Log.Logger("Время окончания парсинга");
        }

        protected void ParserDocument(IDocument t)
        {
            try
            {
                t.ParsingDocument();
            }
            catch (Exception e)
            {
                Log.Logger($"Exception in {t.GetType()}", e);
            }
        }

        protected void CreateDbIfNotExist(string dbName)
        {
            var client = new MongoClient(Builder.ConnectString);
            var dataBases = client.ListDatabases().ToList();
            if (dataBases.All(d => d["name"].AsString != dbName))
            {
                client.GetDatabase(dbName);
            }
        }
        
        protected  List<JToken> GetElements(JToken j, string s)
        {
           var els = new List<JToken>();
            var elsObj = j.SelectToken(s);
            if (elsObj == null || elsObj.Type == JTokenType.Null) return els;
            switch (elsObj.Type)
            {
                case JTokenType.Object:
                    els.Add(elsObj);
                    break;
                case JTokenType.Array:
                    els.AddRange(elsObj);
                    break;
            }

            return els;
        }
    }
}