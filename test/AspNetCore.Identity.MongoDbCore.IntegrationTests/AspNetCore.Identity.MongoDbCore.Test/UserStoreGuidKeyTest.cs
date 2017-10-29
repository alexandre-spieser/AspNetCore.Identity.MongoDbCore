// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository;
using AspNetCore.Identity.MongoDbCore.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.MongoDbCore.Test
{
    public class GuidUser : MongoIdentityUser<Guid>
    {
        public GuidUser() : base()
        {
        }
    }

    public class GuidRole : MongoIdentityRole<Guid>
    {
        public GuidRole() : base()
        {
        }
    }

    public class UserStoreGuidTest : MongoDbStoreTestBase<GuidUser, GuidRole, Guid>
    {
        public UserStoreGuidTest(MongoDatabaseFixture<GuidUser, GuidRole, Guid> fixture)
            : base(fixture)
        {
        }

        public class ApplicationUserStore : MongoUserStore<GuidUser, GuidRole, IMongoDbContext, Guid>
        {
            public ApplicationUserStore(IMongoDbContext context) : base(Container.MongoRepository.Context) { }
        }

        public class ApplicationRoleStore : MongoRoleStore<GuidRole, IMongoDbContext, Guid>
        {
            public ApplicationRoleStore(IMongoDbContext context) : base(Container.MongoRepository.Context) { }
        }

        protected override void AddUserStore(IServiceCollection services, object context = null)
        {
            services.AddSingleton<IUserStore<GuidUser>>(new ApplicationUserStore(Container.MongoRepository.Context));
        }

        protected override void AddRoleStore(IServiceCollection services, object context = null)
        {
            services.AddSingleton<IRoleStore<GuidRole>>(new ApplicationRoleStore(Container.MongoRepository.Context));
        }

    }
}