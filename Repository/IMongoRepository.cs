using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SplashScraper.model;

namespace SplashScraper.Repository {


    public interface IMongoRepository<T> where T: MongoEntity {

        IMongoDatabase Database { get; }

        IMongoCollection<T> Collection { get; }

        IEnumerable<T> List(Expression<Func<T, bool>> expression);

        T FindById(ObjectId id);

        T Single(Expression<Func<T, bool>> expression);

        void InsertOne(T entity);

        void InsertMany(IEnumerable<T> entities);

        DeleteResult DeleteOne(Expression<Func<T, bool>> filter);

        DeleteResult DeleteMany(Expression<Func<T, bool>> filter);

        UpdateResult UpdateOne(Expression<Func<T, bool>> filter, Expression<Func<T, object>> updateExpression, object newValue);

        UpdateResult UpdateMany(Expression<Func<T,bool>> filter, Expression<Func<T, object>> updateExpression, object newValue);

        ReplaceOneResult ReplaceOne(T entity);

        void ReplaceMany(IEnumerable<T> entities);

        bool EntityExists(Expression<Func<T,bool>> filter);

    }

}