using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDbGenericRepository;
using System;

namespace AspNetCore.Identity.MongoDbCore.Extensions
{
    /// <summary>
    /// Contains extension methods to <see cref="IServiceCollection"/> for adding mongoDb Identity.
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Configures the MongoDb Identity store adapters for the types of TUser only from <see cref="MongoIdentityUser{TKey}"/>.
        /// </summary>
        /// <typeparam name="TUser">The type representing a user.</typeparam>
        /// <typeparam name="TKey">The type of the primary key of the identity document.</typeparam>
        /// <param name="services">The collection of service descriptors.</param>
        /// <param name="mongoDbIdentityConfiguration">A configuration object of the AspNetCore.Identity.MongoDbCore package.</param>
        public static void ConfigureMongoDbIdentityUserOnly<TUser, TKey>(
            this IServiceCollection services,
            MongoDbIdentityConfiguration mongoDbIdentityConfiguration)
                where TUser : MongoIdentityUser<TKey>, new()
                where TKey : IEquatable<TKey>
        {
            ValidateMongoDbSettings(mongoDbIdentityConfiguration.MongoDbSettings);
            CommonMongoDbSetup<TUser, MongoIdentityRole<TKey>, TKey>(services, mongoDbIdentityConfiguration);
        }


        /// <summary>
        /// Configures the MongoDb Identity store adapters for the types of TUser only inheriting from <see cref="MongoIdentityUser"/>.
        /// </summary>
        /// <typeparam name="TUser">The type representing a user.</typeparam>
        /// <param name="services">The collection of service descriptors.</param>
        /// <param name="mongoDbIdentityConfiguration">A configuration object of the AspNetCore.Identity.MongoDbCore package.</param>
        public static void ConfigureMongoDbIdentity<TUser>(this IServiceCollection services, MongoDbIdentityConfiguration mongoDbIdentityConfiguration)
                    where TUser : MongoIdentityUser, new()
        {
            ValidateMongoDbSettings(mongoDbIdentityConfiguration.MongoDbSettings);
            CommonMongoDbSetup<TUser, MongoIdentityRole, Guid>(services, mongoDbIdentityConfiguration);
        }

        /// <summary>
        /// Validates the MongoDbSettings
        /// </summary>
        /// <param name="mongoDbSettings"></param>
        private static void ValidateMongoDbSettings(MongoDbSettings mongoDbSettings)
        {
            if (mongoDbSettings == null)
            {
                throw new ArgumentNullException(nameof(mongoDbSettings));
            }

            if (string.IsNullOrEmpty(mongoDbSettings.ConnectionString))
            {
                throw new ArgumentNullException(nameof(mongoDbSettings.ConnectionString));
            }

            if (string.IsNullOrEmpty(mongoDbSettings.DatabaseName))
            {
                throw new ArgumentNullException(nameof(mongoDbSettings.DatabaseName));
            }
        }

        /// <summary>
        /// Configures the MongoDb Identity store adapters for the types of TUser and TRole.
        /// </summary>
        /// <typeparam name="TUser">The type representing a user.</typeparam>
        /// <typeparam name="TRole">The type representing a role.</typeparam>
        /// <typeparam name="TKey">The type of the primary key of the identity document.</typeparam>
        /// <param name="services">The collection of service descriptors.</param>
        /// <param name="mongoDbIdentityConfiguration">A configuration object of the AspNetCore.Identity.MongoDbCore package.</param>
        /// <param name="mongoDbContext">An object representing a MongoDb connection.</param>
        public static void ConfigureMongoDbIdentity<TUser, TRole, TKey>(this IServiceCollection services, MongoDbIdentityConfiguration mongoDbIdentityConfiguration,
            IMongoDbContext mongoDbContext = null)
                    where TUser : MongoIdentityUser<TKey>, new()
                    where TRole : MongoIdentityRole<TKey>, new()
                    where TKey : IEquatable<TKey>
        {
            ValidateMongoDbSettings(mongoDbIdentityConfiguration.MongoDbSettings);

            if(mongoDbContext == null)
            {
                services.AddIdentity<TUser, TRole>()
                        .AddMongoDbStores<TUser, TRole, TKey>(
                            mongoDbIdentityConfiguration.MongoDbSettings.ConnectionString,
                            mongoDbIdentityConfiguration.MongoDbSettings.DatabaseName)
                        .AddDefaultTokenProviders();
            }
            else
            {
                services.AddIdentity<TUser, TRole>()
                        .AddMongoDbStores<IMongoDbContext>(mongoDbContext)
                        .AddDefaultTokenProviders();
            }

            if (mongoDbIdentityConfiguration.IdentityOptionsAction != null)
            {
                services.Configure(mongoDbIdentityConfiguration.IdentityOptionsAction);
            }
        }


        private static void CommonMongoDbSetup<TUser, TRole, TKey>(this IServiceCollection services, MongoDbIdentityConfiguration mongoDbIdentityConfiguration)
                    where TUser : MongoIdentityUser<TKey>, new()
                    where TRole : MongoIdentityRole<TKey>, new()
                    where TKey : IEquatable<TKey>
        {
            services.AddIdentity<TUser, TRole>()
                    .AddMongoDbStores<TUser, TRole, TKey>(
                        mongoDbIdentityConfiguration.MongoDbSettings.ConnectionString, 
                        mongoDbIdentityConfiguration.MongoDbSettings.DatabaseName)
                    .AddDefaultTokenProviders();

            if (mongoDbIdentityConfiguration.IdentityOptionsAction != null)
            {
                services.Configure(mongoDbIdentityConfiguration.IdentityOptionsAction);
            }
        }
    }
}
