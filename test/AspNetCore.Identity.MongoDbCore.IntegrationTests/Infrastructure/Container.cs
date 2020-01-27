using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.Extensions.Configuration;
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
        public static IConfiguration Configuration { get; set; }

        static Container()
        {
            var builder = new ConfigurationBuilder()
                                    .SetBasePath(System.Environment.CurrentDirectory)
                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                    //per user config that is not committed to repo, use this to override settings (e.g. connection string) based on your local environment.
                                    .AddJsonFile($"appsettings.local.json", optional: true); 

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();

            var databaseSettings = Configuration.Load<MongoDbSettings>("MongoDbSettings");

            MongoDbIdentityConfiguration = new MongoDbIdentityConfiguration()
            {
                MongoDbSettings = databaseSettings,
                IdentityOptionsAction = (options) =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.AllowedUserNameCharacters = null;
                }
            };

            lock (Locks.MongoInitLock)
            {
                _mongoDbRepository = new MongoRepository(
                                        databaseSettings.ConnectionString,
                                        databaseSettings.DatabaseName);
                _mongoDbRepository2 = new MongoRepository(
                        databaseSettings.ConnectionString,
                        databaseSettings.DatabaseName);
            }
        }

        public static MongoDbIdentityConfiguration MongoDbIdentityConfiguration { get; set; }

        public static IServiceProvider Instance { get; set; }

        const string connectionString = "mongodb://localhost:27017";
        private static readonly IMongoRepository _mongoDbRepository;

        private static readonly IMongoRepository _mongoDbRepository2;

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

    public static class ConfigurationExtensions
    {
        public static T Load<T>(this IConfiguration configuration, string key) where T : new()
        {
            var instance = new T();
            configuration.GetSection(key).Bind(instance);
            return instance;
        }

        public static T Load<T>(this IConfiguration configuration, string key, T instance) where T : new()
        {
            configuration.GetSection(key).Bind(instance);
            return instance;
        }
    }
}
