// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using AspNetCore.Identity.MongoDbCore.Models;

namespace AspNetCore.Identity.MongoDbCore.Test
{
    public class IntUser : MongoIdentityUser<int>
    {
        public IntUser() : base()
        {
        }
    }

    public class IntRole : MongoIdentityRole<int>
    {
        public IntRole() : base()
        {
            Name = Guid.NewGuid().ToString();
        }
    }

    public class UserStoreIntTest : MongoDbStoreTestBase<IntUser, IntRole, int>
    {
        public UserStoreIntTest(MongoDatabaseFixture<IntUser, IntRole, int> fixture)
            : base(fixture)
        {
        }
    }
}