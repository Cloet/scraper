using System;
using MongoDB.Bson;

namespace SplashScraper.model {

    public class Crypto: MongoEntity {

        public string CryptoName { get; set; }

        public Crypto() {

        }

        public Crypto(string name) {
            CryptoName = name;
        }

        public string CryptoShortName {get; set;}

        public DateTime DateTime { get; set; }

        public double MarketCap { get; set; }

        public double Price { get; set; }

        public double Volume { get; set; }

        public double Circulating { get; set; }

        public double Change { get; set; }

        public string Currency { get; set; }

    }

}