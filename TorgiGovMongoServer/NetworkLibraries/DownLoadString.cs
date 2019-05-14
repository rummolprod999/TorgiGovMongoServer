using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TorgiGovMongoServer.Logger;

namespace TorgiGovMongoServer.NetworkLibraries
{
    public class DownLoadString
    {
        public static class DownloadString
        {
            public static string DownL(string url)
            {
                var tmp = "";
                var count = 0;
                while (true)
                {
                    try
                    {
                        var task = Task.Run(() => (new TimedWebClient()).DownloadString(url));
                        if (!task.Wait(TimeSpan.FromSeconds(650))) throw new TimeoutException();
                        tmp = task.Result;
                        break;
                    }

                    catch (Exception e)
                    {
                        if (count >= 3)
                        {
                            Log.Logger($"Не удалось скачать за {count} попыток", url);
                            break;
                        }

                        switch (e)
                        {
                            case AggregateException a
                                when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                                Log.Logger("404 Exception", a.InnerException.Message, url);
                                return tmp;
                            case AggregateException a
                                when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                                Log.Logger("403 Exception", a.InnerException.Message, url);
                                return tmp;
                            case AggregateException a when a.InnerException != null &&
                                                           a.InnerException.Message.Contains(
                                                               "The remote server returned an error: (434)"):
                                Log.Logger("434 Exception", a.InnerException.Message, url);
                                return tmp;
                        }

                        Log.Logger("Не удалось получить строку", e, url);
                        count++;
                        Thread.Sleep(5000);
                    }
                }

                return tmp;
            }

            public static string DownLUserAgent(string url)
            {
                var tmp = "";
                var count = 0;
                while (true)
                {
                    try
                    {
                        var task = Task.Run(() => (new TimedWebClientUa()).DownloadString(url));
                        if (!task.Wait(TimeSpan.FromSeconds(100))) throw new TimeoutException();
                        tmp = task.Result;
                        break;
                    }
                    catch (WebException ex)
                    {
                        if (ex.Response is HttpWebResponse r) Log.Logger("Response code: ", r.StatusCode);
                        if (ex.Response is HttpWebResponse errorResponse &&
                            errorResponse.StatusCode == HttpStatusCode.Forbidden)
                        {
                            Log.Logger("Error 403 or 434");
                            return tmp;
                        }

                        if (count >= 5)
                        {
                            Log.Logger($"Не удалось скачать за {count} попыток", url);
                            break;
                        }

                        Log.Logger("Не удалось получить строку", ex.Message, url);
                        count++;
                        Thread.Sleep(5000);
                    }
                    catch (Exception e)
                    {
                        if (count >= 5)
                        {
                            Log.Logger($"Не удалось скачать за {count} попыток", url);
                            break;
                        }

                        switch (e)
                        {
                            case AggregateException a
                                when a.InnerException != null && a.InnerException.Message.Contains("(404) Not Found"):
                                Log.Logger("404 Exception", a.InnerException.Message, url);
                                goto Finish;
                            case AggregateException a
                                when a.InnerException != null && a.InnerException.Message.Contains("(403) Forbidden"):
                                Log.Logger("403 Exception", a.InnerException.Message, url);
                                goto Finish;
                            case AggregateException a when a.InnerException != null &&
                                                           a.InnerException.Message.Contains(
                                                               "The remote server returned an error: (434)"):
                                Log.Logger("434 Exception", a.InnerException.Message, url);
                                goto Finish;
                        }

                        Log.Logger("Не удалось получить строку", e, url);
                        count++;
                        Thread.Sleep(5000);
                    }
                }

                Finish:
                return tmp;
            }

            public static string DownL1251(string url)
            {
                var tmp = "";
                var count = 0;
                while (true)
                {
                    try
                    {
                        var task = Task.Run(() =>
                        {
                            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                            var v = new TimedWebClient {Encoding = Encoding.GetEncoding("windows-1251")};
                            return v.DownloadString(url);
                        });
                        if (!task.Wait(TimeSpan.FromSeconds(650))) throw new TimeoutException();
                        tmp = task.Result;
                        break;
                    }
                    catch (Exception e)
                    {
                        if (count >= 3)
                        {
                            Log.Logger($"Не удалось скачать за {count} попыток", url);
                            break;
                        }

                        Log.Logger("Не удалось получить строку", e, url);
                        count++;
                        Thread.Sleep(5000);
                    }
                }

                return tmp;
            }
        }
    }
}