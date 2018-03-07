// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository;
using AspNetCore.Identity.MongoDbCore.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;

namespace AspNetCore.Identity.MongoDbCore.Test
{
    public class ObjectIdUser : MongoIdentityUser<ObjectId>
    {
        public ObjectIdUser() : base()
        {
        }
    }

    public class ObjectIdRole : MongoIdentityRole<ObjectId>
    {
        public ObjectIdRole() : base()
        {
        }
    }

    public class UserStoreObjectIdTest : MongoDbStoreTestBase<ObjectIdUser, ObjectIdRole, ObjectId>
    {
        public UserStoreObjectIdTest(MongoDatabaseFixture<ObjectIdUser, ObjectIdRole, ObjectId> fixture)
            : base(fixture)
        {
        }

        public class ApplicationUserStore : MongoUserStore<ObjectIdUser, ObjectIdRole, IMongoDbContext, ObjectId>
        {
            public ApplicationUserStore(IMongoDbContext context) : base(Container.MongoRepository.Context) { }
        }

        public class ApplicationRoleStore : MongoRoleStore<ObjectIdRole, IMongoDbContext, ObjectId>
        {
            public ApplicationRoleStore(IMongoDbContext context) : base(Container.MongoRepository.Context) { }
        }

        protected override void AddUserStore(IServiceCollection services, object context = null)
        {
            services.AddSingleton<IUserStore<ObjectIdUser>>(new ApplicationUserStore(Container.MongoRepository.Context));
        }

        protected override void AddRoleStore(IServiceCollection services, object context = null)
        {
            services.AddSingleton<IRoleStore<ObjectIdRole>>(new ApplicationRoleStore(Container.MongoRepository.Context));
        }

    }
}