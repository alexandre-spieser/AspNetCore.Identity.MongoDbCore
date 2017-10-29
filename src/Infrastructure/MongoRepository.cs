using MongoDbGenericRepository;

namespace AspNetCore.Identity.MongoDbCore.Infrastructure
{
    /// <summary>
    /// The repository used in the MongoDb identity stores.
    /// </summary>
    public interface IMongoRepository : IBaseMongoRepository
    {
        /// <summary>
        /// Drops a collections.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document used to define the collection name.</typeparam>
        void DropCollection<TDocument>();

        /// <summary>
        /// Drops a partitioned collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document used to define the collection name.</typeparam>
        /// <param name="partitionKey">The partition key of the collection.</param>
        void DropCollection<TDocument>(string partitionKey);

        /// <summary>
        /// The MongoDb context.
        /// </summary>
        IMongoDbContext Context { get; }
    }

    /// <summary>
    /// The repository used in the MongoDb identity stores.
    /// </summary>
    public class MongoRepository : BaseMongoRepository, IMongoRepository
    {
        /// <summary>
        /// The constructor for <see cref="MongoRepository"/> requiring a connection string and a database name.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="databaseName">The database name.</param>
        public MongoRepository(string connectionString, string databaseName) : base(connectionString, databaseName)
        {
        }

        /// <summary>
        /// The constructor for <see cref="MongoRepository"/> requiring a <see cref="IMongoDbContext"/>.
        /// </summary>
        /// <param name="mongoDbContext">A <see cref="IMongoDbContext"/>.</param>
        public MongoRepository(IMongoDbContext mongoDbContext) : base(mongoDbContext)
        {
        }

        /// <summary>
        /// Drops a collections.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document used to define the collection name.</typeparam>
        public void DropCollection<TDocument>()
        {
            MongoDbContext.DropCollection<TDocument>();
        }

        /// <summary>
        /// Drops a partitioned collection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document used to define the collection name.</typeparam>
        /// <param name="partitionKey">The partition key of the collection.</param>
        public void DropCollection<TDocument>(string partitionKey)
        {
            MongoDbContext.DropCollection<TDocument>(partitionKey);
        }

        /// <summary>
        /// The MongoDb context.
        /// </summary>
        public IMongoDbContext Context => MongoDbContext;
    }
}
