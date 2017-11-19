// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDbGenericRepository;
using AspNetCore.Identity.MongoDbCore;
using AspNetCore.Identity.MongoDbCore.Models;
using AspNetCore.Identity.MongoDbCore.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods to <see cref="IdentityBuilder"/> for adding MongoDb stores.
    /// </summary>
    public static class MongoDbIdentityBuilderExtensions
    {
        /// <summary>
        /// Adds an MongoDb implementation of identity information stores.
        /// </summary>
        /// <typeparam name="TContext">The MongoDb database context to use.</typeparam>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <param name="mongoDbContext">A mongoDbContext</param>
        /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddMongoDbStores<TContext>(this IdentityBuilder builder, IMongoDbContext mongoDbContext)
            where TContext : IMongoDbContext
        {
            if(mongoDbContext == null)
            {
                throw new ArgumentNullException(nameof(mongoDbContext));
            }

            builder.Services.TryAddSingleton<IMongoDbContext>(mongoDbContext);
            builder.Services.TryAddSingleton<IMongoRepository>(new MongoRepository(mongoDbContext));

            AddStores(builder.Services, builder.UserType, builder.RoleType, typeof(TContext));
            return builder;
        }

        /// <summary>
        /// Adds an MongoDb implementation of identity information stores.
        /// </summary>
        /// <typeparam name="TUser">The type representing a user.</typeparam>
        /// <typeparam name="TRole">The type representing a role.</typeparam>
        /// <typeparam name="TKey">The type of the primary key of the identity document.</typeparam>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <param name="connectionString"></param>
        /// <param name="databaseName"></param>
        public static IdentityBuilder AddMongoDbStores<TUser, TRole, TKey>(this IdentityBuilder builder, string connectionString, string databaseName)
                    where TUser : MongoIdentityUser<TKey>, new()
                    where TRole : MongoIdentityRole<TKey>, new()
                    where TKey : IEquatable<TKey>
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException(nameof(databaseName));
            }
            builder.Services.TryAddSingleton<MongoDbSettings>(new MongoDbSettings
            {
                ConnectionString = connectionString,
                DatabaseName = databaseName
            });
            builder.AddMongoDbStores<TUser, TRole, TKey>(new MongoDbContext(connectionString, databaseName));
            return builder;
        }

        /// <summary>
        /// Adds an MongoDb implementation of identity information stores.
        /// </summary>
        /// <typeparam name="TUser">The type representing a user.</typeparam>
        /// <typeparam name="TRole">The type representing a role.</typeparam>
        /// <typeparam name="TKey">The type of the primary key of the identity document.</typeparam>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <param name="mongoDbContext"></param>
        public static IdentityBuilder AddMongoDbStores<TUser, TRole, TKey>(this IdentityBuilder builder, IMongoDbContext mongoDbContext)
                    where TUser : MongoIdentityUser<TKey>, new()
                    where TRole : MongoIdentityRole<TKey>, new()
                    where TKey : IEquatable<TKey>
        {
            if (mongoDbContext == null)
            {
                throw new ArgumentNullException(nameof(mongoDbContext));
            }

            builder.Services.TryAddSingleton<IMongoDbContext>(mongoDbContext);
            builder.Services.TryAddSingleton<IMongoRepository>(new MongoRepository(mongoDbContext));
            builder.Services.TryAddScoped<IUserStore<TUser>>(provider =>
            {
                return new MongoUserStore<TUser, TRole, IMongoDbContext, TKey>(provider.GetService<IMongoDbContext>());
            });

            builder.Services.TryAddScoped<IRoleStore<TRole>>(provider =>
            {
                return new MongoRoleStore<TRole, IMongoDbContext, TKey>(provider.GetService<IMongoDbContext>());
            });
            return builder;
        }

        private static void AddStores(IServiceCollection services, Type userType, Type roleType, Type contextType)
        {
            var identityUserType = FindGenericBaseType(userType, typeof(MongoIdentityUser<>));
            if (identityUserType == null)
            {
                throw new InvalidOperationException(Resources.NotIdentityUser);
            }

            var keyType = identityUserType.GenericTypeArguments[0];

            if (roleType != null)
            {
                var identityRoleType = FindGenericBaseType(roleType, typeof(MongoIdentityRole<>));
                if (identityRoleType == null)
                {
                    throw new InvalidOperationException(Resources.NotIdentityRole);
                }

                Type userStoreType = null;
                Type roleStoreType = null;

                // If its a custom DbContext, we can only add the default POCOs
                userStoreType = typeof(MongoUserStore<,,,>).MakeGenericType(userType, roleType, contextType, keyType);
                roleStoreType = typeof(MongoRoleStore<,,>).MakeGenericType(roleType, contextType, keyType);

                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
                services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
            }
            else
            {   // No Roles
                Type userStoreType = null;
                // If its a custom DbContext, we can only add the default POCOs
                userStoreType = typeof(MongoUserStore<,,,>).MakeGenericType(userType, roleType, contextType, keyType);
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            }

        }

        private static TypeInfo FindGenericBaseType(Type currentType, Type genericBaseType)
        {
            var type = currentType;
            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (genericType != null && genericType == genericBaseType)
                {
                    return typeInfo;
                }
                type = type.BaseType;
            }
            return null;
        }
    }
}