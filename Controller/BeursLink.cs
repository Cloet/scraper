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

    public class BeursLink {

        public string Link { get; private set;}

        public string Endpoint {get; private set;}

        public string Script {get; private set;}

        public BeursLink(string splashEndpoint, string script, string link) {
            Endpoint = splashEndpoint;
            Script = script;
            Link = link;
        }


        public async Task StartAsync() {
            // Start scraping
            var html = await SplashController.ScrapeWebPageToString(Endpoint, Link, Script);

            // Main item
            await ScrapeMain(html);
        }

        private async Task ScrapeMain(string html) {
            // Clean html
            html = HtmlEntity.DeEntitize((String)JObject.Parse(html)["1"]);

            if (string.IsNullOrEmpty(html)) {
                ControllerHelper.WriteLine("No data was found.",true);
                return;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var table = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class,'SimpleTable') and contains(@class,'issue')]")?.FirstOrDefault();

            if (table == null)
                return;

            var head = table.SelectNodes("./thead");
            var body = table.SelectNodes("./tbody");

            foreach(var b in body) {
                var rows = b.SelectNodes("./tr");
                // iterate over each row in the body.
                // Each row at this stage represents a different stock.
                foreach(var row in rows ?? Enumerable.Empty<HtmlNode>()) {
                    var data = row.SelectNodes("./td");
                    var stock = CollectStockData(data, false, 2020,1).Result;
                    // Collect historical data of the stock.
                    if (stock != null) {
                        var repo = RepoManager.BeursRepository(stock.Name.Replace(" ", "_") + "_(" + stock.StockName.Replace(" ", "_") + ")");
                        repo.InsertOne(stock);

                        WriteData(stock);
                        await CollectHistoricalStockData(stock,repo);
                    }

                }
            }
        }

        private async Task CollectHistoricalStockData(Beurs beurs, IMongoRepository<Beurs> repository) {
            var pages = 50;

            if (string.IsNullOrEmpty(beurs.HistoLink))
                return;
            
            var histoLink = beurs.HistoLink.Replace(".aspx","/historische-koersen.aspx?maand=");
            var items = new List<Beurs>();

            // Historieken van aandeel overlopen
            for (var i = 0; i < pages; i++) {             
                try {
                    var histo = histoLink + i.ToString();

                    // Clean html
                    var html = await SplashController.ScrapeWebPageToString(Endpoint,histo, Script);
                    html = HtmlEntity.DeEntitize((String)JObject.Parse(html)["1"]);

                    if (string.IsNullOrEmpty(html)) {
                        ControllerHelper.WriteLine("No data was found.",true);
                        return;
                    }

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(html);
                    
                    var year = 2020;
                    var month = 1;
                    
                    var tables = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'ContentLeft')]")?.FirstOrDefault();
                    if (tables == null)
                        return;

                    var title = tables.SelectNodes("./h2")?.FirstOrDefault();
                    if (title != null) {
                        var time = title.InnerText.Split("-");
                        year = (int) GetNumbers(SanitizeString(time[1]));
                        var monthS = (SanitizeString(time[1]).Replace(year.ToString(),"")).Trim();
                        month = DateTime.ParseExact(monthS,"MMMM", CultureInfo.GetCultureInfo("nl-be")).Month;
                    }

                    var table = tables.SelectNodes("./table[contains(@class,'SimpleTable')]")?.FirstOrDefault();

                    if (table == null)
                        return;

                    var head = table.SelectNodes("./thead");
                    var body = table.SelectNodes("./tbody");

                    foreach(var b in body) {
                        var rows = b.SelectNodes("./tr");
                        // iterate over each row in the body.
                        // Each row at this stage represents a different stock.
                        items = new List<Beurs>();
                        foreach(var row in rows ?? Enumerable.Empty<HtmlNode>()) {
                            var data = row.SelectNodes("./td");
                            var stock = await CollectStockData(data, true, year,month);
                            // Collect historical data of the stock.
                            stock.Index = beurs.Index;
                            stock.StockName = beurs.StockName;
                            stock.StockNumber = beurs.StockNumber;
                            stock.Name = beurs.Name;
                            WriteData(stock);
                            items.Add(stock);
                        }

                        // Insert all items.
                        repository.InsertMany(items);
                    }
                } catch (Exception ex) {
                    ControllerHelper.WriteLine("Failed to retrieve historical data." + ex.ToString(),true);
                    if (items.Count > 0) {
                        repository.InsertMany(items);
                        items = new List<Beurs>();
                    }
                }
            }

        }

        private void WriteData(Beurs beurs) {
            var builder = new StringBuilder();
            builder.Append($"{beurs.Name} @ {beurs.DateTime.ToString("dd-MM-yyyy")} @ {beurs.DateTime.ToString("T")}");
            builder.Append($"{Environment.NewLine}        Volume      : {beurs.Volume}");
            builder.Append($"{Environment.NewLine}        Change      : {beurs.Difference}");
            builder.Append($"{Environment.NewLine}        Change (%)  : {beurs.PercentageDifference}");
            builder.Append($"{Environment.NewLine}        High        : {beurs.High}");
            builder.Append($"{Environment.NewLine}        Low         : {beurs.Low}");
            builder.Append($"{Environment.NewLine}        Currency    : {beurs.Currency}");
            builder.Append($"{Environment.NewLine}        StockName   : {beurs.StockName}");
            builder.Append($"{Environment.NewLine}        Index       : {beurs.Index}");
            builder.Append($"{Environment.NewLine}        Number      : {beurs.StockNumber}");
            ControllerHelper.WriteLine(builder.ToString());
        }

        private async Task<Beurs> CollectStockData(HtmlNodeCollection data, bool collectHistorical, int year, int month) {
            
            var i = 0;
            Beurs beurs = new Beurs();
            beurs.Currency = "USD";

            try {
                foreach (var d in data) {
                    var value = d.InnerText;

                    switch(collectHistorical) {
                        case true:
                            if (i == 0)
                                beurs.DateTime = new DateTime(year,month, (int) GetNumbers(value));
                            else if (i == 1)
                                beurs.Open = GetNumbers(value);
                            else if (i == 2)
                                beurs.Close = GetNumbers(value);
                            else if (i == 3)
                                beurs.Low = GetNumbers(value);
                            else if (i == 5)
                                beurs.High = GetNumbers(value);
                            else if (i == 6)
                                beurs.Volume = GetNumbers(value);
                            else if (i == 7)
                                beurs.Difference = GetNumbers(value);
                            else if (i == 8)
                                beurs.PercentageDifference = GetNumbers(value);
                            break;
                        case false:
                            beurs.DateTime = DateTime.Now;
                            if (i == 0) {
                                beurs.Name = SanitizeString(value).Replace("...","");
                                var link = d.Descendants("a")?.FirstOrDefault();
                                if (link != null) {
                                    beurs.HistoLink = link.Attributes["href"].Value.Replace("../../../","https://www.beursduivel.be/");
                                }
                            } else if (i == 2)
                                beurs.Difference = GetNumbers(value);
                            else if (i == 3)
                                beurs.PercentageDifference = GetNumbers(value);
                            else if (i == 4)
                                beurs.High = GetNumbers(value);
                            else if (i == 5)
                                beurs.Low = GetNumbers(value);
                            else if (i == 6)
                                beurs.Close = GetNumbers(value);

                            if (string.IsNullOrEmpty(beurs.StockName) && !string.IsNullOrEmpty(beurs.HistoLink)) {
                                beurs = GatherAdditionalInfo(beurs, await SplashController.ScrapeWebPageToString(Endpoint,beurs.HistoLink,Script));
                                if (string.IsNullOrEmpty(beurs.StockName))
                                    beurs.StockName = beurs.Name.Replace(" ", "_");
                            }      
                            break;

                    }

                    i++;
                }

                return beurs;
            } catch (Exception) {
                return null;
            }

            
        }

        private Beurs GatherAdditionalInfo(Beurs beurs, string html) {
            try {
                var o = JObject.Parse(html);
                html = HtmlEntity.DeEntitize((string)o["1"]);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var stock = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'IssueTitle')]")?.FirstOrDefault();

                if (stock != null) {

                    var name = stock.SelectNodes("./h1")?.FirstOrDefault();
                    if (name != null)
                        beurs.Name = name.InnerText.ToString().Trim();
                    
                    var span = stock.SelectNodes("./span")?.FirstOrDefault();
                    if (span != null) {
                        var text = span.InnerText;
                        var textArray = text.Split(",");
                        var secondArray = textArray[0].Split(":");

                        beurs.Index = secondArray != null && secondArray.Length > 0 ? secondArray[0].Trim() : "";
                        beurs.StockName = secondArray != null && secondArray.Length >= 1 ? secondArray[1].Trim() : "";
                        beurs.StockNumber = textArray != null && textArray.Length >= 1 ? textArray[1].Trim() : "";
                        return beurs;
                    }
                }
                return beurs;
            } catch (Exception ex) {
                ControllerHelper.WriteLine(ex.ToString(), true);
                return beurs;
            }
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