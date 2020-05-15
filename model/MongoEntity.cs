using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SplashScraper.model {

    public abstract class MongoEntity : IMongoEntity
    {

        public ObjectId Id { get ; set; }
    }

}