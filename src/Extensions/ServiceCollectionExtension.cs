using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDbGenericRepository;
using System;

namespace AspNetCore.Identity.MongoDbCore.Extensions
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public class MongoDbIdentityConfiguration
    {
        public MongoDbSettings MongoDbSettings { get; set; }
        public Action<IdentityOptions> IdentityOptionsAction { get; set; }
    }

    public static class ServiceCollectionExtension
    {
        public static void ConfigureMongoDbIdentity<TUser, TKey>(
            this IServiceCollection services, 
            MongoDbIdentityConfiguration mongoDbIdentityConfiguration, 
            IMongoRepository mongoRepository = null)
                where TUser : MongoIdentityUser<TKey>, new()
                where TKey : IEquatable<TKey>
        {
            services.AddSingleton<MongoDbSettings>(mongoDbIdentityConfiguration.MongoDbSettings);
            services.AddSingleton<IMongoRepository>(provider =>
            {
                var options = provider.GetService<MongoDbSettings>();
                return mongoRepository ?? new MongoRepository(options.ConnectionString, options.DatabaseName);
            });

            CommonMongoDbSetup<TUser, MongoIdentityRole<TKey>, TKey>(services, mongoDbIdentityConfiguration);
        }

        public static void ConfigureMongoDbIdentity<TUser>(this IServiceCollection services, MongoDbIdentityConfiguration mongoDbIdentityConfiguration)
                    where TUser : MongoIdentityUser, new()
        {
            services.AddSingleton<MongoDbSettings>(mongoDbIdentityConfiguration.MongoDbSettings);
            services.AddSingleton<IMongoRepository>(provider =>
            {
                var options = provider.GetService<MongoDbSettings>();
                return new MongoRepository(options.ConnectionString, options.DatabaseName);
            });

            CommonMongoDbSetup<TUser, MongoIdentityRole, Guid>(services, mongoDbIdentityConfiguration);
        }


        public static void ConfigureMongoDbIdentity<TUser, TRole, TKey>(this IServiceCollection services, MongoDbIdentityConfiguration mongoDbIdentityConfiguration,
            IMongoDbContext mongoDbContext = null)
                    where TUser : MongoIdentityUser<TKey>, new()
                    where TRole : MongoIdentityRole<TKey>, new()
                    where TKey : IEquatable<TKey>
        {
            services.AddSingleton<MongoDbSettings>(mongoDbIdentityConfiguration.MongoDbSettings);
            services.AddSingleton<IMongoRepository>(provider =>
            {
                var options = provider.GetService<MongoDbSettings>();
                return mongoDbContext == null ? new MongoRepository(options.ConnectionString, options.DatabaseName) : new MongoRepository(mongoDbContext);
            });

            CommonMongoDbSetup<TUser, TRole, TKey>(services, mongoDbIdentityConfiguration);
        }

        private static void CommonMongoDbSetup<TUser, TRole, TKey>(this IServiceCollection services, MongoDbIdentityConfiguration mongoDbIdentityConfiguration)
                    where TUser : MongoIdentityUser<TKey>, new()
                    where TRole : MongoIdentityRole<TKey>, new()
                    where TKey : IEquatable<TKey>
        {
            services.AddScoped<IUserStore<TUser>>(provider =>
            {
                var userStore = new MongoUserStore<TUser, TRole, IMongoDbContext, TKey>(provider.GetService<IMongoRepository>().Context);
                return userStore;
            });

            services.AddScoped<IRoleStore<TRole>>(provider =>
            {
                return new MongoRoleStore<TRole, IMongoDbContext, TKey>(provider.GetService<IMongoRepository>().Context);
            });

            services.AddIdentity<TUser, TRole>()
                    .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(mongoDbIdentityConfiguration.IdentityOptionsAction);
        }

    }
}
