using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SplashScraper.model;

namespace SplashScraper.Repository {


    public class MongoRepository<T>: IMongoRepository<T> where T: MongoEntity {

        public IMongoDatabase Database { get => Context.Database; }

        public IMongoCollection<T> Collection { get; private set; }

        public IMongoContext Context { get; private set;}

        public MongoRepository(IMongoContext context) {
            Context = context;
            Collection = Database.GetCollection<T>(typeof(T).Name.ToLower());
        }

        public MongoRepository(IMongoContext context, string collection) {
            Context = context;
            Collection = Database.GetCollection<T>(collection.ToLower());
        }

        public IEnumerable<T> List(Expression<Func<T, bool>> filter){
            return Collection.AsQueryable<T>().Where(filter);
        }

        public T FindById(ObjectId id) {
            return Single(x => x.Id == id);
        }

        public T Single(Expression<Func<T, bool>> filter) {
            return List(filter).SingleOrDefault();
        }

        public bool EntityExists(Expression<Func<T,bool>> filter) {
            var item = Collection.Find(filter).ToList();
            
            return (item.Count > 0);
        }

        public void InsertOne(T entity){
            Collection.InsertOne(entity);
        }

        public void InsertMany(IEnumerable<T> entities){
            Collection.InsertMany(entities);
        }

        public DeleteResult DeleteOne(Expression<Func<T, bool>> filter){
            return Collection.DeleteOne(filter);
        } 


        public DeleteResult DeleteMany(Expression<Func<T, bool>> filter){
            return Collection.DeleteMany(filter);
        }

        public UpdateResult UpdateOne(Expression<Func<T, bool>> filter, Expression<Func<T, object>> updateExpression, object newValue) {
            
            var update = Builders<T>.Update.Set(updateExpression, newValue);

            return Collection.UpdateOne(filter, update);
        }

        public UpdateResult UpdateMany(Expression<Func<T,bool>> filter, Expression<Func<T, object>> updateExpression, object newValue) {
            var update = Builders<T>.Update.Set(updateExpression, newValue);

            return Collection.UpdateMany(filter, update);
        }

        public ReplaceOneResult ReplaceOne(T entity) {
            
            if (entity.Id == null)
                throw new ArgumentNullException(nameof(entity.Id));

            return Collection.ReplaceOne(x => x.Id == entity.Id, entity);
        }

        public void ReplaceMany(IEnumerable<T> entities) {
            
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            
            foreach (var entity in entities) {
                ReplaceOne(entity);
            }
        }


    }

}