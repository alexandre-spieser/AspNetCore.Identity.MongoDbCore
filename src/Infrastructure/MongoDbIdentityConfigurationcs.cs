using Microsoft.AspNetCore.Identity;
using System;

namespace AspNetCore.Identity.MongoDbCore.Infrastructure
{
    /// <summary>
    /// A class used to perform a full configuration of the AspNetCore.Identity.MongoDbCore package.
    /// </summary>
    public class MongoDbIdentityConfiguration
    {
        /// <summary>
        /// The settings for the MongoDb server.
        /// </summary>
        public MongoDbSettings MongoDbSettings { get; set; }
        /// <summary>
        /// An action against an <see cref="IdentityOptions"/> to change the default identity settings.
        /// </summary>
        public Action<IdentityOptions> IdentityOptionsAction { get; set; }
    }
}
