using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace XECrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            int numberOfPages = Int32.Parse(ConfigurationManager.AppSettings["numberOfPages"]);

            List<string> urls = ConfigurationManager.AppSettings["propertySearchPageUrls"].Split('|').ToList();

            List<UrlModel> modelUrls = new List<UrlModel>();

            modelUrls.Add(new UrlModel() { Url = urls[0], Name = "Residence" });
            modelUrls.Add(new UrlModel() { Url = urls[1], Name = "Land" });
            modelUrls.Add(new UrlModel() { Url = urls[2], Name = "Professional" });

            var fileNameList = new List<string>();
            var pagesNewIds = new List<string>();
            foreach (UrlModel url in modelUrls)
            {
                using (var client = new HttpClient())
                {
                    Console.WriteLine($"Searching {url.Name}...");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; B-l-i-t-z-B-O-T)");
                    string username = ConfigurationManager.AppSettings["xeUsername"];
                    string password = ConfigurationManager.AppSettings["xePassword"];

                    //if you use CookieAwareWebClient
                    //client.Encoding = System.Text.Encoding.UTF8;

                    #region Login
                    //if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                    //{
                    //    var values = new NameValueCollection
                    //    {
                    //        {"email",username},
                    //        {"password",password},
                    //        {"autologin","1"},
                    //        {"redirect","http://www.xe.gr/"},
                    //    };
                    //    client.UploadValues("https://my.xe.gr/app/login", values);
                    //    Cookie jSessionID = client.ResponseCookies["JSESSIONID"];
                    //    if (jSessionID != null)
                    //    {
                    //        // The server set a cookie called JSESSIONID, you can use it here:
                    //        string value = jSessionID.Value;
                    //        Console.WriteLine(jSessionID);
                    //    }
                    //    Cookie _XERemLog = client.ResponseCookies["_XERemLog"];
                    //    if (_XERemLog != null)
                    //    {
                    //        // The server set a cookie called _XERemLog, you can use it here:
                    //        string value = _XERemLog.Value;
                    //        Console.WriteLine(_XERemLog);
                    //    }
                    //    WebHeaderCollection myWebHeaderCollection = client.ResponseHeaders;
                    //    Console.WriteLine("\nREQUEST HEADERS:");
                    //    for (int i = 0; i < myWebHeaderCollection.Count; i++)
                    //        Console.WriteLine("\n" + myWebHeaderCollection.GetKey(i) + " = " + myWebHeaderCollection.Get(i));
                    //}
                    #endregion

                    var searchNewIds = CrawlingFunctions.GetNewPropertyIds(numberOfPages, client, url.Url).Result.Distinct().ToList();
                    pagesNewIds.AddRange(searchNewIds);

                    if (searchNewIds.Count() > 0)
                    {
                        List<PropertyModel> properties = CrawlingFunctions.GetProperties(searchNewIds, client).Result;

                        string fileName = ExcelExport.ExportToFile(properties, url);
                        fileNameList.Add(fileName);
                        Console.WriteLine($"{searchNewIds.Count()} new properties found for {url.Name}.");
                    }
                    else
                    {
                        Console.WriteLine($"No new properties found for {url.Name}.");
                    }
                };
            }

            EmailSender.SendMail(fileNameList, pagesNewIds.Count()).Wait();
            SaveIds(pagesNewIds);

            Console.WriteLine($"{pagesNewIds.Count()} new properties --> END");
            Console.WriteLine("Press any to exit.");
            Console.ReadKey();
        }

        public static void SaveIds(List<string> ids)
        {
            try
            {
                using (var context = new XECrawlerEntities())
                {
                    var exportedIds = ids.Select(id => new ExportedProperties() { ExportedPropertyId = id });
                    context.ExportedProperties.AddRange(exportedIds);
                    context.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception message: {ex.Message}, Inner Exception: {ex.InnerException}");
            }
            
        }
    }
}
