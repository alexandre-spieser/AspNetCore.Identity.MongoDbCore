using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Driver;
using MongoDbGenericRepository;
using System;

namespace AspNetCore.Identity.MongoDbCore
{

    public class MongoIdentityDbContext : MongoDbContext
    {

        public MongoIdentityDbContext(string connectionString, string databaseName) : base(connectionString, databaseName)
        {
        }

        public IMongoRepository MongoRepository { get; }

        public IMongoCollection<MongoIdentityUser> Users
        {
            get
            {
                return GetCollection<MongoIdentityUser>();
            }
        }

        public IMongoCollection<MongoIdentityRole> Roles
        {
            get
            {
                return GetCollection<MongoIdentityRole>();
            }
        }
    }

    public class MongoIdentityDbContext<TUser> : MongoDbContext
        where TUser : MongoIdentityUser
    {

        public MongoIdentityDbContext(string connectionString, string databaseName) : base(connectionString, databaseName)
        {
        }

        public IMongoRepository MongoRepository { get; }

        public IMongoCollection<TUser> Users
        {
            get
            {
                return GetCollection<TUser>();
            }
        }

        public IMongoCollection<MongoIdentityRole> Roles
        {
            get
            {
                return GetCollection<MongoIdentityRole>();
            }
        }
    }

    public class MongoIdentityDbContext<TUser, TRole, TKey> : MongoDbContext
            where TUser : MongoIdentityUser<TKey>
            where TRole : MongoIdentityRole<TKey>
            where TKey : IEquatable<TKey>
    {

        public MongoIdentityDbContext(string connectionString, string databaseName) : base(connectionString, databaseName)
        {
        }

        public IMongoCollection<TUser> Users
        {
            get
            {
                return GetCollection<TUser>();
            }
        }

        public IMongoCollection<TRole> Roles
        {
            get
            {
                return GetCollection<TRole>();
            }
        }
    }
}
