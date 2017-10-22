using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using System;

namespace AspNetCore.Identity.MongoDbCore.IntegrationTests.Infrastructure
{
    public static class Locks
    {
        public static object MongoInitLock = new object();
        public static object RolesLock = new object();
    }

    public static class Container
    {
        public static MongoDbIdentityConfiguration MongoDbIdentityConfiguration = new MongoDbIdentityConfiguration
        {
            MongoDbSettings = new MongoDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "MongoDbTests"
            },
            IdentityOptionsAction = options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.AllowedUserNameCharacters = null;
            }
        };

        public static IServiceProvider Instance { get; set; }

        const string connectionString = "mongodb://localhost:27017";
        private static readonly IMongoRepository _mongoDbRepository;

        private static readonly IMongoRepository _mongoDbRepository2;

        static Container()
        {
            lock (Locks.MongoInitLock)
            {
                _mongoDbRepository = new MongoRepository(
                                        MongoDbIdentityConfiguration.MongoDbSettings.ConnectionString,
                                        MongoDbIdentityConfiguration.MongoDbSettings.DatabaseName);
                _mongoDbRepository2 = new MongoRepository(
                        MongoDbIdentityConfiguration.MongoDbSettings.ConnectionString,
                        MongoDbIdentityConfiguration.MongoDbSettings.DatabaseName);
            }
        }

        public static IMongoRepository MongoRepository
        {
            get
            {
                return _mongoDbRepository;
            }
        }

        public static IMongoRepository MongoRepositoryConcurrent
        {
            get
            {
                return _mongoDbRepository2;
            }
        }
    }
}
