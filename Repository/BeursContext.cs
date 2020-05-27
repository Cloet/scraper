using MongoDB.Driver;

namespace SplashScraper.Repository {

    public class BeursContext: IMongoContext
    {
        public IMongoDatabase Database { get; private set;}

        public string ConnectionString { get; private set;}
    
        public BeursContext() {
            ConnectionString = "mongodb://192.168.0.121:27017";
            var client = new MongoClient(ConnectionString);
            Database = client.GetDatabase("beursV7");
        }

    }

}