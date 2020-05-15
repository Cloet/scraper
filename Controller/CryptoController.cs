using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using SplashScraper.Factory;
using SplashScraper.model;
using SplashScraper.Repository;

namespace SplashScraper.Controller {


    public class CryptoController {

        private string _url;

        public string Url { 
            get => _url;
            private set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                _url = value;             
            } 
        }

        public int Port { get; private set; }

        public string EndPoint { get => $"{Url}:{Port}"; }

        private string _htmlFolder = "";

        public CryptoController(string splashUrl, int splashPort, string htmlFolder) {
            RepoManager.Context = new CryptoContext();
            
            Url = splashUrl;
            Port = splashPort;
            if (htmlFolder.Substring(htmlFolder.Length-1,1) != "/")
                htmlFolder += "/";

            _htmlFolder = htmlFolder;

        }

        private string LoadScript() {
            var script = File.ReadAllText("/mnt/Data/School/Bachelorproef/SplashScraper/scripts/script.lua");
            return script;
        }


        public void ScrapeCoinmarketcap() {
            var baseUrl = "https://coinmarketcap.com";

            var url = "";
            Console.WriteLine("Start scraping from coinmarketcap.com");

            for (var i = 1; i < 26; i++) {
                url = baseUrl + $"/{i}/";
                Console.WriteLine($"Scraping page '{url}'.");
                ProcessTable(ScrapeHtmlFromPage(url));
                Console.WriteLine($"{url} has been scraped.");
            }
            Console.WriteLine("Scraping has finished.");
        }

        private string UrlPath(string url) {
            var uri = new Uri(url);
            return uri.AbsolutePath.Replace("//","/").Replace("\\","/").Replace("!","").Replace("?","");
        }

        private Stream ScrapeHtmlFromPage(string urlToScrape) {

            var client = new HttpClient();

            var requestUrl = $"{EndPoint}/execute?url={urlToScrape}&wait=10&timeout=90.0";
            requestUrl = $"{requestUrl}&lua_source={LoadScript()}";

            // Do request
            var response = client.GetAsync(requestUrl).Result;

            var stream = response.Content.ReadAsStreamAsync().Result;

            return stream;
        }

        private void ProcessTable(Stream stream) {

            var file = SaveToTempFile(stream);
            var txt = File.ReadAllText(file);
            var o = JObject.Parse(txt);

            txt = HtmlEntity.DeEntitize((string) o["1"]);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(txt);

            var homepage = htmlDoc.DocumentNode.SelectNodes("//div[@class='cmc-homepage']");
            var first = homepage.Nodes().FirstOrDefault();
            
            first = first.ChildNodes[2];

            var tables = first.SelectNodes(".//table");
            var table = tables.ElementAt(2);

            var head = table.SelectNodes("./thead");
            var body = table.SelectNodes("./tbody");

            try {
                foreach (var b in body) {
                    var rows = b.SelectNodes("./tr");
                    foreach (var row in rows) {
                        var data = row.SelectNodes("./td");
                        int counter = 0;
                        var number = "";
                        Crypto crypto = new Crypto();

                        foreach (var d in data) {
                            crypto.DateTime = DateTime.Now;
                            crypto.Currency = "USD";
                            var val = d.InnerText;

                            if (counter == 0)
                                number = val;

                            if (counter == 1)
                                crypto.CryptoName = val;
                            else if (counter == 2)
                                crypto.MarketCap = GetNumbers(val);
                            else if (counter == 3)
                                crypto.Price = GetNumbers(val);
                            else if (counter == 4)
                                crypto.Volume = GetNumbers(val);
                            else if (counter == 5) {
                                crypto.Circulating = GetNumbers(val);
                                crypto.CryptoShortName = GetCryptoShort(val);
                            }
                            else if (counter == 6)
                                crypto.Change = GetNumbers(val);

                            counter++;
                        }
                        Console.WriteLine($"#{number} -> {crypto.CryptoName} @ {crypto.DateTime.ToString()}");
                        Console.WriteLine($"        Marketcap   : {crypto.MarketCap}");
                        Console.WriteLine($"        Price       : {crypto.Price}");
                        Console.WriteLine($"        Volume (24h): {crypto.Volume}");
                        Console.WriteLine($"        Circulating : {crypto.Circulating}");
                        Console.WriteLine($"        Change (%)  : {crypto.Change}");
                        Console.WriteLine($"        Currency    : {crypto.Currency}");
                        Console.WriteLine($"        Short name  : {crypto.CryptoShortName}");
                        var repo = RepoManager.CryptoRepository(crypto.CryptoName);
                        repo.InsertOne(crypto);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }            

            
            File.Delete(file);
        }

        private static string GetCryptoShort(string input) {
            try {
                var s = new string(input.Where(c => !char.IsDigit(c)).ToArray());
                return s.Replace("*","").Replace(",","").Replace(".","").Trim();
            } catch(Exception){
                return "";
            }
        }

        private static Double GetNumbers(string input)
        {
            try {
                var s = new string(input.Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-').ToArray());
                return Double.Parse(s);
            } catch( Exception) {
                return 0;
            }
        }


        private string SanitizeUrl(string url) {
            return url.Replace("https://","").Replace("http://","").Replace("/","-").Replace("\\","-").Replace("?","").Replace("!","");
        }

        private string FilePath(string url) {
            return $"{_htmlFolder}/{UrlHost(url)}{UrlPath(url)}{FileName(url)}_{DateTime.Now.ToString("dd_MM_yyyy")}";
        }

        private string UrlHost(string url) {
            var uri = new Uri(url);
            return SanitizeUrl(uri.Host);
        }

        private string FileName(string url) {
            var path = UrlPath(url);
            var filename = "";

            if (path.Substring(path.Length-1,1) == "/")
                filename = "index.html";
            else
                filename = path.Substring(path.LastIndexOf("/"), path.Length - path.LastIndexOf("/"));

            return SanitizeUrl(filename);
        }

        private string SaveToTempFile(Stream stream) {

            var file = Path.GetTempFileName();

            using (var fStream = File.Create(file)) {
                stream.CopyTo(fStream);
            }

            return file;

        }

        // public void WriteDataFromHTMLToFile(string html, string path, string url, string origin) {           
        //     var htmlDoc = new HtmlDocument();
        //     htmlDoc.LoadHtml(html);
        // }

        // public void WriteLinksToMongo(string txt, string url, string baseUrl) 
        // {
        //     var htmlDoc = new HtmlDocument();
        //     htmlDoc.LoadHtml(txt);

        //     var items = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
        //     Uri uri = new Uri(url);

        //     if (items != null) {
        //         foreach (var item in items) {
        //             if (item != null) {
        //                 var link = item.GetAttributeValue("href", string.Empty).Replace("\"","").Replace("\\","");
                        
        //                 if (!link.Contains("http"))
        //                     link = $"https://{uri.Host}{link}";

        //                 Link li = new Link(link, baseUrl);

        //                 if (!RepoManager.LinkRepository.EntityExists(x => x.Url == link)) {
        //                     li.Url = link;
        //                     li.Checked = false;
        //                     li.TrackNumber = ++_linkcounter;
        //                     RepoManager.LinkRepository.InsertOne(li);
        //                     Console.WriteLine($"[NEW] {link} has been added to the database for later processing.");
        //                 }
        //                 else
        //                     Console.WriteLine($"[DUPLICATE] {link} is a duplicate and will not be added.");
        //             }
        //         }
        //     }

        //     htmlDoc = null;
        // }
        
    }

}