using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.MongoDbCore
{
    /// <summary>
    /// Represents the password hashing options
    /// </summary>
    public sealed class PasswordHasherOptionsAccessor : IOptions<PasswordHasherOptions>
    {
        /// <summary>
        /// Gets options which use the IdentityV3 compat mode, and set the iteration count to 200000 PBKDF2-SHA256 iterations 
        /// (roughly 200ms of work)
        /// </summary>
        public PasswordHasherOptions Value { get; } = new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
            IterationCount = 200000
        };
    }
}