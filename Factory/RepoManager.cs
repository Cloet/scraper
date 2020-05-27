using System;
using SplashScraper.model;
using SplashScraper.Repository;

namespace SplashScraper.Factory {

    public class RepoManager{
        
        private static readonly Lazy<RepoManager> INSTANCE = new Lazy<RepoManager>(() => new RepoManager());

        public static IMongoContext Context { get; set; }
        
        private readonly RepositoryFactory _factory;

        private RepoManager() {
            if (Context == null)
                Context = new CryptoContext();
            _factory = RepositoryFactory.Initialize(Context);
        }

        public static RepoManager Instance {
            get {
                return INSTANCE.Value;
            }
        }

        public static IMongoRepository<Crypto> CryptoRepository(string name) {
            return Instance._factory.CryptoRepository(name);
        }

        public static IMongoRepository<Beurs> BeursRepository(string name) {
            if (Instance?._factory == null)
                return null;
            return Instance._factory.BeursRepository(name);
        }

        public static IMongoRepository<Sector> SectorRepository() {
            return Instance._factory.SectorRepository("sector");
        }

        private RepositoryFactory _secondFactory = null;

        public static IMongoRepository<Sector> SectorRepositoryFromAnotherContext() {
            if (Instance._secondFactory == null) {
                Instance._secondFactory = RepositoryFactory.Initialize(new SectorContext());
            }

            return Instance._secondFactory.SectorRepository("sector");
        }

    }

}