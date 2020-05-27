using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using SplashScraper.Factory;
using SplashScraper.model;
using SplashScraper.Repository;

namespace SplashScraper.Controller {

    public class SectorLink {

        public string Link { get; private set;}

        public string Endpoint {get; private set;}

        public string Script {get; private set;}

        public SectorLink(string splashEndpoint, string script, string link) {
            Endpoint = splashEndpoint;
            Script = script;
            Link = link;
        }


        public void Start() {
            // Start scraping
            var html = SplashController.ScrapeWebPageToString(Endpoint, Link, Script).Result;

            // Main item
            ScrapeMain(html);
        }

        private void ScrapeMain(string html) {
            // Clean html
            html = HtmlEntity.DeEntitize((String)JObject.Parse(html)["1"]);

            if (string.IsNullOrEmpty(html)) {
                ControllerHelper.WriteLine("No data was found.",true);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var title = htmlDoc.DocumentNode.SelectNodes("//h3")?.FirstOrDefault()?.InnerText.Trim();
            var table = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class,'table') and contains(@class,'top-stocks')]")?.FirstOrDefault();

            if (table == null) {
                ControllerHelper.WriteLine("No table was found...");
                ControllerHelper.WriteLine(html);
                return;
            }


            var head = table.SelectNodes("./thead");
            var body = table.SelectNodes("./tbody");

            var repo = RepoManager.SectorRepository();

            foreach(var b in body) {
                var rows = b.SelectNodes("./tr");
                // iterate over each row in the body.
                // Each row at this stage represents a different stock.
                foreach(var row in rows ?? Enumerable.Empty<HtmlNode>()) {
                    var data = row.SelectNodes("./td");
                    var i = 0;

                    var sector = new Sector(title);
                    foreach (var d in data) {
                        if (i == 1) {
                            sector.StockName = d.InnerText.Trim();
                        }
                        i++;
                    }
                    repo.InsertOne(sector);
                    WriteData(sector);
                }

            }
        }

        private void WriteData(Sector sector) {
            var builder = new StringBuilder();
            builder.Append($"{sector.StockName} @ {sector.SectorName}");
            ControllerHelper.WriteLine(builder.ToString());
        }

        private Double GetNumbers(string input)
        {
            try {
                input = SanitizeString(input);
                var s = new string(input.Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-' ).ToArray());
                return Double.Parse(s, new CultureInfo("nl-be"));
            } catch(Exception) {
                return 0;
            }
        }

        private string SanitizeString(string input) {
            try {
                input = input.Replace("\n","").Replace("\t","");
                input = input.Trim();
                return input;
            } catch (Exception) {
                return input;
            }
        }

        private string SanitizeUrl(string url) {
            return url.Replace("https://","").Replace("http://","").Replace("/","-").Replace("\\","-").Replace("?","").Replace("!","");
        }

    }

}