namespace SplashScraper.model{

    public class Sector : MongoEntity {

        public Sector(string sectorName) {
            SectorName = sectorName;
        }

        public string StockName { get; set; }

        public string SectorName { get; set; }


    }

}