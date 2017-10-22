using MongoDbGenericRepository;

namespace AspNetCore.Identity.MongoDbCore.Infrastructure
{
    public interface IMongoRepository : IBaseMongoRepository
    {
        void DropCollection<TDocument>();
        void DropCollection<TDocument>(string partitionKey);
        IMongoDbContext Context { get; }
    }


    public class MongoRepository : BaseMongoRepository, IMongoRepository
    {
        public MongoRepository(string connectionString, string databaseName) : base(connectionString, databaseName)
        {
        }

        public MongoRepository(IMongoDbContext mongoDbContext) : base(mongoDbContext)
        {
        }

        public void DropCollection<TDocument>()
        {
            MongoDbContext.DropCollection<TDocument>();
        }

        public void DropCollection<TDocument>(string partitionKey)
        {
            MongoDbContext.DropCollection<TDocument>(partitionKey);
        }

        public IMongoDbContext Context => MongoDbContext;
    }
}
