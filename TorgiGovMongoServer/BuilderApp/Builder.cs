using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TorgiGovMongoServer.BuilderApp
{
    public class Builder
    {
        [Required] public static string TempDir { get; set; }
        [Required] public static string LogDir { get; set; }
        [Required] public static string FileLog { get; set; }
        [Required] public static string UserDb { get; set; }
        [Required] public static string PassDb { get; set; }
        [Required] public static string Server { get; set; }
        [Required] public static string Database { get; set; }
        [Required] public static string ConnectString { get; set; }
        private static int _port;
        public static Arguments Arg { get; private set; }
        private static Builder _b;

        public const string ReqArguments =
            "last, curr";

        public static readonly string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName()
            .CodeBase.Substring(5));

        private Builder(string arg)
        {
            GetArgument(arg);
            GetSettings();
            CreateDirs();
        }

        private static void GetArgument(string s)
        {
            switch (s)
            {
                case "curr":
                    Arg = Arguments.Curr;
                    break;
                case "last":
                    Arg = Arguments.Last;
                    break;
                default:
                    throw new Exception($"Неправильно указан аргумент {s}, используйте {ReqArguments}");
            }
        }

        private static void GetSettings()
        {
            var nameFile = $"{Path}{System.IO.Path.DirectorySeparatorChar}settings.json";
            using (var reader = File.OpenText(nameFile))
            {
                var o = (JObject) JToken.ReadFrom(new JsonTextReader(reader));
                UserDb = (string) o["userdb"];
                PassDb = (string) o["passdb"];
                Server = (string) o["server"];
                _port = int.TryParse((string) o["port"], out _port) ? int.Parse((string) o["port"]) : 3306;
                Database = (string) o["database"];
                const string logDirTmp = "log_torgi_gov";
                const string tempDirTmp = "temp_torgi_gov";
                LogDir = $"{Path}{System.IO.Path.DirectorySeparatorChar}{logDirTmp}";
                TempDir = $"{Path}{System.IO.Path.DirectorySeparatorChar}{tempDirTmp}";
                FileLog = $"{LogDir}{System.IO.Path.DirectorySeparatorChar}{Arg}_{DateTime.Now:dd_MM_yyyy}.log";
                ConnectString =
                    "mongodb://localhost:27017";
            }
        }

        private static void CreateDirs()
        {
            if (Directory.Exists(TempDir))
            {
                var dirInfo = new DirectoryInfo(TempDir);
                dirInfo.Delete(true);
                Directory.CreateDirectory(TempDir);
            }
            else
            {
                Directory.CreateDirectory(TempDir);
            }

            if (!Directory.Exists(LogDir))
            {
                Directory.CreateDirectory(LogDir);
            }
        }

        public static void GetBuilder(string arg)
        {
            if (_b != null) return;
            _b = new Builder(arg);
            var results = new List<ValidationResult>();
            var context = new ValidationContext(_b);
            if (Validator.TryValidateObject(_b, context, results, true)) return;
            foreach (var error in results)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            Environment.Exit(0);
        }

        public static void DeleteOldLogs()
        {
            var currDate = DateTime.Now;
            var dirLog = new DirectoryInfo((LogDir));
            foreach (var f in dirLog.EnumerateFiles())
            {
                if (f.CreationTime.AddDays(30) <= currDate)
                {
                    f.Delete();
                }
            }
        }
    }
}