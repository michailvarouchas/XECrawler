using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XECrawler
{
    public class CrawlingFunctions
    {
        private static Random _random = new Random();

        /// <summary>
        /// Σκανάρει τη σελίδες αναζήτησης της ΧΕ 
        /// </summary>
        /// <returns>Tα ids που δεν έχουμε ήδη κατεβάσει</returns>
        public async static Task<List<string>> GetNewPropertyIds(int pages, HttpClient client, string url)
        {
            var allPropertyIds = new List<string>();

            for (int i = 1; i <= pages; i++)
            {
                Console.WriteLine("Loading page " + i + " from " + pages);

                try
                {
                    var html = await client.GetStringAsync(new Uri(url + "&page=" + i));
                    //wait
                    int s = _random.Next(1, 14);
                    Thread.Sleep(s * 1000);

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    var pageItems = htmlDocument.DocumentNode.Descendants("div")
                        .Where(n => n.GetAttributeValue("class", "").Contains("lazy"))
                        .Select(id => id.GetAttributeValue("data-id", "")).ToList();

                    if (pageItems.Count() == 0)
                    {
                        Console.WriteLine($"\t no properties found on page {i}");
                        break;
                    }

                    allPropertyIds.AddRange(pageItems);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception message: {ex.Message}, Inner Exception: {ex.InnerException}");
                }

            }

            //select only non-exported propertyIds
            var newPropertyIds = new List<string>();

            try
            {
                using (var context = new XECrawlerEntities())
                {
                    var expoertedPropertyIds = context.ExportedProperties.Select(p => p.ExportedPropertyId).ToList();

                    newPropertyIds = allPropertyIds.Where(i => !expoertedPropertyIds.Contains(i)).ToList();
                }

                return newPropertyIds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception message: {ex.Message}, Inner Exception: {ex.InnerException}");
            }
            return null;
        }

        /// <summary>
        /// Σκανάρει τη σελίδες πωλήσεις ακινήτων της ΧΕ επιστρέφει το αντικείμενο που μας ενδιαφέρει
        /// </summary>
        /// <param name="idList"></param>
        /// <returns>Τα χαρακτηρηστικά των ακινήτων</returns>
        public async static Task<List<PropertyModel>> GetProperties(List<string> idList, HttpClient client)
        {
            string url = ConfigurationManager.AppSettings["propertyDetailsPageUrl"];

            var list = new List<HtmlNode>();
            var propertyList = new List<PropertyModel>();

            int z = 1;
            foreach (var item in idList)
            {
                //wait
                int s = _random.Next(1, 14);
                Thread.Sleep(s * 1000);

                int nProperties = idList.Count;
                Console.WriteLine($"Receiving property {z} from {nProperties}. Id: {item}");
                var propertyItem = new PropertyModel();

                try
                {
                    var html = await client.GetStringAsync(new Uri(url + item + ".html"));

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    var pageItems = htmlDocument.DocumentNode.Descendants("div")
                        .Where(n => n.GetAttributeValue("id", "").Equals("d_container"))
                        .FirstOrDefault();

                    var pageHeaderItems = htmlDocument.DocumentNode.Descendants("div")
                        .Where(n => n.GetAttributeValue("id", "").Equals("ad_details_summary"))
                        .FirstOrDefault();

                    var phoneItem = htmlDocument.DocumentNode.Descendants("div")
                        .Where(n => n.GetAttributeValue("id", "").Equals("ad_details_phone"))
                        .FirstOrDefault();

                    //get the id
                    propertyItem.Id = item;

                    //get the title parts
                    var title = pageItems.SelectSingleNode("h4").InnerText;

                    //property type
                    propertyItem.PropertyType = title.GetStartOrEmpty(",");

                    //sq meters
                    if (title.GetMiddleOrEmpty(",", "στην περιοχή").TrimStart(' ').Contains("στρ"))
                    {
                        if (Double.TryParse(title.GetMiddleOrEmpty(",", "στην περιοχή").TrimStart(' ').TrimEnd('σ', 'τ', 'ρ', '.'), out double stremata))
                        {
                            propertyItem.SqMeteters = stremata * 1000;
                        }
                    }
                    else
                    {
                        if (Double.TryParse(title.GetMiddleOrEmpty(",", "στην περιοχή").TrimStart(' ').TrimEnd('τ', '.', 'μ'), out double sqm))
                        {
                            propertyItem.SqMeteters = sqm;
                        }
                    }

                    //location
                    var fullLocation = title.GetEndOrEmpty("στην περιοχή").TrimStart(' ');
                    var locationParts = fullLocation.Split('>');
                    if (locationParts.Count() > 0)
                        propertyItem.Locationp1 = locationParts[0].TrimEnd(' ');
                    if (locationParts.Count() > 1)
                        propertyItem.Locationp2 = locationParts[1].TrimStart(' ').TrimEnd(' ');
                    if (locationParts.Count() > 2)
                        propertyItem.Locationp2 = locationParts[2].TrimStart(' ');

                    //get the description
                    propertyItem.Description = pageItems.SelectSingleNode("p").InnerText.Trim(' ');

                    //get foor(s)
                    string[] terms = { "1ου", "2ου", "3ου", "4ου", "5ου", "6ου", "7ου", "8ου", "9ου", "10ου", "11ου", "12ου", "13ου", "14ου", "15ου", "16ου", "17ου" };
                    string floors = "";
                    foreach (var term in terms)
                    {
                        floors += propertyItem.Description.Contains(term) ? " " + term : "";
                    }
                    propertyItem.Floor = floors.TrimStart(' ');
                    
                    //get price
                    var pricetext = pageHeaderItems.SelectSingleNode("div/table/tbody/tr/td[2]").InnerText;
                    if (Double.TryParse(pricetext.TrimStart('\n', ' ').TrimEnd(' ', ';', 'o', 'r', 'u', 'e', '&'), out double price))
                        propertyItem.Price = price;

                    //get the properties
                    var properties = pageItems.SelectNodes("ul/li");

                    //set the properties
                    if (properties != null)
                    {
                        foreach (var property in properties)
                        {
                            var prp = property.InnerHtml.ToString();
                            var innerPrp = property.SelectSingleNode("span").InnerHtml.ToString();

                            if (prp.Contains("Έτος Κατασκευής"))
                            {
                                if (Int32.TryParse(innerPrp, out int propYear))
                                    propertyItem.Year = propYear;
                            }
                            if (prp.Contains("Υπνοδωμάτια"))
                            {
                                if (Int32.TryParse(innerPrp, out int propBed))
                                    propertyItem.Bedroms = propBed;
                            }
                            if (prp.Contains("Μπάνια"))
                            {
                                if (Int32.TryParse(innerPrp, out int propRooms))
                                    propertyItem.Toilets = propRooms;
                            }
                            if (prp.Contains("Πάρκιν"))
                            {
                                propertyItem.Parking = innerPrp;
                            }
                            if (prp.Contains("Τζάκι"))
                            {
                                propertyItem.Fireplace = innerPrp;
                            }
                            if (prp.Contains("Αυτόνομη θέρμανση"))
                            {
                                propertyItem.AutonomousHeat = innerPrp;
                            }
                        }
                    }
                    
                    //get phone from json object
                    string phoneJson = await client.GetStringAsync(new Uri($"http://www.xe.gr/property/phoneimg?sys_id={item}&text=1"));
                    var xePhoneObject = JsonConvert.DeserializeObject<XEphoneModel>(phoneJson);
                    if (xePhoneObject != null && xePhoneObject.data != null)
                    {
                        propertyItem.Phone = xePhoneObject.data.phone;
                    }

                    //add to list
                    propertyList.Add(propertyItem);
                    z++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception message: {ex.Message}, Inner Exception: {ex.InnerException}");
                    z++;
                }
            }
            return propertyList;
        }
    }
}
