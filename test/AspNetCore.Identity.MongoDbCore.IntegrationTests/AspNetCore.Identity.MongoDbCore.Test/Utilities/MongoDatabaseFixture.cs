// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using MongoDbGenericRepository;
using AspNetCore.Identity.MongoDbCore.IntegrationTests.Infrastructure;
using System.Collections.Concurrent;
using System.Linq;
using MongoDB.Driver;
using MongoDbGenericRepository.Models;

namespace AspNetCore.Identity.MongoDbCore.Test
{
    public class MongoDatabaseFixture<TUser, TKey> : IDisposable
        where TUser : IDocument<TKey>
        where TKey : IEquatable<TKey>
    {

        public IMongoDbContext Context;

        public MongoDatabaseFixture()
        {
            Context = new MongoDbContext(
                Container.MongoDbIdentityConfiguration.MongoDbSettings.ConnectionString,
                Container.MongoDbIdentityConfiguration.MongoDbSettings.DatabaseName);
            UsersToDelete = new ConcurrentBag<TUser>();
        }
        public ConcurrentBag<TUser> UsersToDelete { get; set; }
        public virtual void Dispose()
        {
            var userIds = UsersToDelete.ToList().Select(e => e.Id);
            if (userIds.Any())
            {
                Context.GetCollection<TUser>().DeleteMany(e => userIds.Contains(e.Id));
            }
        }
    }

    public class MongoDatabaseFixture<TUser, TRole, TKey> : MongoDatabaseFixture<TUser, TKey>, IDisposable
        where TUser : IDocument<TKey>
        where TRole : IDocument<TKey>
        where TKey : IEquatable<TKey>
    {

        public MongoDatabaseFixture()
        {
            Context = new MongoDbContext(
                Container.MongoDbIdentityConfiguration.MongoDbSettings.ConnectionString,
                Container.MongoDbIdentityConfiguration.MongoDbSettings.DatabaseName);
            UsersToDelete = new ConcurrentBag<TUser>();
            RolesToDelete = new ConcurrentBag<TRole>();
        }
        public ConcurrentBag<TRole> RolesToDelete { get; set; }

        public override void Dispose()
        {
            var userIds = UsersToDelete.ToList().Select(e => e.Id);
            if (userIds.Any())
            {
                Context.GetCollection<TUser>().DeleteMany(e => userIds.Contains(e.Id));
            }
            var roleIds = RolesToDelete.ToList().Select(e => e.Id);
            if (roleIds.Any())
            {
                Context.GetCollection<TRole>().DeleteMany(e => roleIds.Contains(e.Id));
            }
        }
    }
}