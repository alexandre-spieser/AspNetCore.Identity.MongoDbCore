// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using AspNetCore.Identity.MongoDbCore.Models;

namespace AspNetCore.Identity.MongoDbCore.Test
{
    public class LongUser : MongoIdentityUser<long>
    {
        public LongUser() : base()
        {
        }
    }

    public class LongRole : MongoIdentityRole<long>
    {
        public LongRole() : base()
        {
            Name = Guid.NewGuid().ToString();
        }
    }

    public class UserStoreLongTest : MongoDbStoreTestBase<LongUser, LongRole, long>
    {
        public UserStoreLongTest(MongoDatabaseFixture<LongUser, LongRole, long> fixture)
            : base(fixture)
        {
        }
    }
}