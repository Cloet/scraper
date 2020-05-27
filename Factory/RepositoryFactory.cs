using SplashScraper.Repository;
using SplashScraper.model;
using System.Collections.Generic;

namespace SplashScraper.Factory {

    public class RepositoryFactory {

        private IMongoContext _context;

        private IDictionary<string, IMongoRepository<Crypto>> _cryptoRepositories = new Dictionary<string, IMongoRepository<Crypto>>();

        private IDictionary<string, IMongoRepository<Beurs>> _beursRepositories = new Dictionary<string, IMongoRepository<Beurs>>();

        private IDictionary<string, IMongoRepository<Sector>> _sectorRepositories = new Dictionary<string, IMongoRepository<Sector>>();

        private RepositoryFactory(IMongoContext context) {
            _context = context;
        }

        public static RepositoryFactory Initialize(IMongoContext context) => new RepositoryFactory(context);

        public IMongoRepository<Beurs> BeursRepository(string name) {
            // var found = _beursRepositories.TryGetValue(name, out var repository);

            // if (!found) {
            //     repository = new MongoRepository<Beurs>(_context, name);
            //     _beursRepositories.Add(name, repository);
            // }
            // return repository;
            
            return new MongoRepository<Beurs>(_context,name);
        }

        public IMongoRepository<Crypto> CryptoRepository(string cryptoName) {
            var found = _cryptoRepositories.TryGetValue(cryptoName, out var repository);
            
            if (!found) {
                repository = new MongoRepository<Crypto>(_context, cryptoName);
                _cryptoRepositories.Add(cryptoName, repository);
            }

            return repository;
        }

        public IMongoRepository<Sector> SectorRepository(string stockName) {
            // var found = _sectorRepositories.TryGetValue(stockName, out var repository);

            // if (!found) {
            //     repository = new MongoRepository<Sector>(_context, stockName);
            //     _sectorRepositories.Add(stockName, repository);
            // }

            return new MongoRepository<Sector>(_context,stockName);
        }

    }

}