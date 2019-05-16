using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TorgiGovMongoServer.BuilderApp;

namespace TorgiGovMongoServer.Logger
{
    public static class Log
    {
        private static string _fileLog;
        private static object _locker = new object();

        static Log()
        {
            _fileLog = Builder.FileLog;
        }

        public static void Logger(params object[] parametrs)
        {
            var s = "";
            s += DateTime.Now.ToString(CultureInfo.InvariantCulture);
            s = parametrs.Aggregate(s, (current, t) => $"{current} {t}");

            lock (_locker)
            {
                using (var sw = new StreamWriter(_fileLog, true, Encoding.Default))
                {
                    sw.WriteLine(s);
                }
            }
        }
    }
}