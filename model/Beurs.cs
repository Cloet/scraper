
using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SplashScraper.model{

    public class Beurs : MongoEntity {

        public Beurs() {

        }

        public string Currency { get; set; }

        public string Name { get; set; }

        public string StockName { get; set; }

        public string Index { get; set; }

        public string StockNumber { get; set; }

        public DateTime DateTime { get; set; }

        public double Difference { get; set; }

        public double PercentageDifference { get; set; }

        public double Volume { get; set; }

        public double High { get; set;}

        public double Low { get; set; }

        public double Open { get; set; }

        public double Close { get; set; }


        [BsonIgnore]
        public string HistoLink { get; set; }


    }

}