using AspNetCore.Identity.MongoDbCore.Models;
using System;

namespace AspNetCore.Identity.MongoDbCore.IntegrationTests.Infrastructure
{
    public class TestMongoIdentityRole : MongoIdentityRole<Guid>
    {
        public TestMongoIdentityRole() : base ()
        {
            Id = Guid.NewGuid();
        }

        public TestMongoIdentityRole(string roleName) : base(roleName)
        {
            Id = Guid.NewGuid();
        }
    }
}
