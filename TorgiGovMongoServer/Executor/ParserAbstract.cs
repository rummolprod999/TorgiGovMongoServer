using System;
using TorgiGovMongoServer.Documents;
using TorgiGovMongoServer.Logger;

namespace TorgiGovMongoServer.Executor
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
    }
}