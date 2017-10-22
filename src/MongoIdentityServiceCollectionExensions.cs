using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

    public static class MongoIdentityServiceCollectionExtensions
    {
        public static IdentityBuilder AddIdentity<TUser>(this IServiceCollection services)
            where TUser : class => services.AddIdentity<TUser>(null);

        public static IdentityBuilder AddIdentity<TUser>(this IServiceCollection services, Action<IdentityOptions> setupAction)
            where TUser : class
        {
            // Hosting doesn't add IHttpContextAccessor by default
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Identity services
            services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
            services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();

            services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();

            // No interface for the error describer so we can add errors without rev'ing the interface
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser>>();
            services.TryAddScoped<UserManager<TUser>, AspNetUserManager<TUser>>();
            services.TryAddScoped<SignInManager<TUser>, SignInManager<TUser>>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return new IdentityBuilder(typeof(TUser), services);
        }
    }
}
