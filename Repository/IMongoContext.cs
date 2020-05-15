using MongoDB.Driver;

namespace SplashScraper.Repository {

    public interface IMongoContext
    {
        IMongoDatabase Database { get; }

        string ConnectionString { get; }
    }

}