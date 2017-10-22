using AspNetCore.Identity.MongoDbCore.Models;
using System;

namespace AspNetCore.Identity.MongoDbCore.IntegrationTests.Infrastructure
{
    public class TestMongoIdentityUser : MongoIdentityUser<Guid>
    {
        public TestMongoIdentityUser() : base()
        {
            Id = Guid.NewGuid();
        }

        public TestMongoIdentityUser(string userName) : base(userName)
        {
            Id = Guid.NewGuid();
        }

        public TestMongoIdentityUser(string userName, string email) : base(userName, email)
        {
            Id = Guid.NewGuid();
        }

        public string CustomContent { get; set; }
    }
}
