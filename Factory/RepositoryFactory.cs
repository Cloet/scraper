using SplashScraper.Repository;
using SplashScraper.model;
using System.Collections.Generic;

namespace SplashScraper.Factory {

    public class RepositoryFactory {

        private static IMongoContext _context;

        private static IDictionary<string, IMongoRepository<Crypto>> _cryptoRepositories = new Dictionary<string, IMongoRepository<Crypto>>();

        private static IDictionary<string, IMongoRepository<Beurs>> _beursRepositories = new Dictionary<string, IMongoRepository<Beurs>>();

        private RepositoryFactory(IMongoContext context) {
            _context = context;
        }

        public static RepositoryFactory Initialize(IMongoContext context) => new RepositoryFactory(context);

        public IMongoRepository<Beurs> BeursRepository(string name) {
            var found = _beursRepositories.TryGetValue(name, out var repository);

            if (!found) {
                repository = new MongoRepository<Beurs>(_context, name);
                _beursRepositories.Add(name, repository);
            }

            return repository;
        }

        public IMongoRepository<Crypto> CryptoRepository(string cryptoName) {
            var found = _cryptoRepositories.TryGetValue(cryptoName, out var repository);
            
            if (!found) {
                repository = new MongoRepository<Crypto>(_context, cryptoName);
                _cryptoRepositories.Add(cryptoName, repository);
            }

            return repository;
        }

    }

}