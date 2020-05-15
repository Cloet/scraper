using MongoDB.Bson;

namespace SplashScraper.model {

    public interface IMongoEntity
    {
        ObjectId Id { get; set; }

    }

}