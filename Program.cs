using System;
using SplashScraper.Controller;

namespace SplashScraper
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Stock/Crypto Scraper.");
            var input = Choice();

            if (input == "S") {
                var controller = new BeursController("http://192.168.0.121",8050, "/home/cloet/Projects/school/bachelorproef/SplashScraper/data");
                controller.ScrapeBeurs();
            }

            if (input == "C") {
                var controller = new CryptoController("http://192.168.0.121",8050, "/home/cloet/Projects/school/bachelorproef/SplashScraper/data");
                controller.ScrapeCoinmarketcap();
            }

            if (input == "E") {
                var controller = new SectorController("http://192.168.0.121",8050,"/home/cloet/Projects/school/bachelorproef/SplashScraper/data");
                controller.ScrapeSectoren();
            }
            
        }

        private static string Choice() {
            Console.Write("Scrape crypto's (C),stocks (S) or sectors (E): ");
            var input = Console.ReadLine().ToUpper();

            if (input.Trim() == "S" || input.Trim() == "C" || input.Trim() == "E") {
                return input.Trim();
            }
            else {
                Console.WriteLine("Invalid choice choose 'C','S' or 'E'.");
                return Choice();
            }
        }
    }
}
