using System;
using SplashScraper.Controller;

namespace SplashScraper
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Stock/Sector Scraper.");
            var input = Choice();

            if (input == "S") {
                var controller = new BeursController("splash.mathiascloet.com",8050, "/home/cloet/Projects/school/bachelorproef/SplashScraper/data");
                controller.ScrapeBeurs();
            }

            if (input == "E") {
                var controller = new SectorController("splash.mathiascloet.com",8050,"/home/cloet/Projects/school/bachelorproef/SplashScraper/data");
                controller.ScrapeSectoren();
            }
            
        }

        private static string Choice() {
            Console.Write("Scrape stocks (S) or sectors (E): ");
            var input = Console.ReadLine().ToUpper();

            if (input.Trim() == "S" || input.Trim() == "E") {
                return input.Trim();
            }
            else {
                Console.WriteLine("Invalid choice choose,'S' or 'E'.");
                return Choice();
            }
        }
    }
}
