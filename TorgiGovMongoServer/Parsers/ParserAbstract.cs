using System;
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
    }
}