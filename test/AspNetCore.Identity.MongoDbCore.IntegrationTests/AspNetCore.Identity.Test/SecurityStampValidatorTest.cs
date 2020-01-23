// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using AspNetCore.Identity.MongoDbCore.IntegrationTests;

namespace Microsoft.AspNetCore.Identity.Test
{
    public class SecurityStampTest
    {
        private class NoopHandler : IAuthenticationHandler
        {
            public Task<AuthenticateResult> AuthenticateAsync()
            {
                throw new NotImplementedException();
            }

            public Task ChallengeAsync(AuthenticationProperties properties)
            {
                throw new NotImplementedException();
            }

            public Task ForbidAsync(AuthenticationProperties properties)
            {
                throw new NotImplementedException();
            }

            public Task<bool> HandleRequestAsync()
            {
                throw new NotImplementedException();
            }

            public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
            {
                throw new NotImplementedException();
            }

            public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
            {
                throw new NotImplementedException();
            }

            public Task SignOutAsync(AuthenticationProperties properties)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task OnValidatePrincipalThrowsWithEmptyServiceCollection()
        {
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.RequestServices).Returns(new ServiceCollection().BuildServiceProvider());
            var id = new ClaimsPrincipal(new ClaimsIdentity(IdentityConstants.ApplicationScheme));
            var ticket = new AuthenticationTicket(id, new AuthenticationProperties { IssuedUtc = DateTimeOffset.UtcNow }, IdentityConstants.ApplicationScheme);
            var context = new CookieValidatePrincipalContext(httpContext.Object, new AuthenticationSchemeBuilder(IdentityConstants.ApplicationScheme) { HandlerType = typeof(NoopHandler) }.Build(), new CookieAuthenticationOptions(), ticket);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => SecurityStampValidator.ValidatePrincipalAsync(context));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OnValidatePrincipalTestSuccess(bool isPersistent)
        {
            var user = new TestUser { UserName = "test" };

            var manager = SetupUserManager(user);
            manager.Setup(m => m.SupportsUserLockout).Returns(true).Verifiable();
            manager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true).Verifiable();

            var context = new Mock<HttpContext>();

            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.HttpContext).Returns(context.Object);

            var roleManager = MockHelpers.MockRoleManager<TestRole>();

            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(a => a.Value).Returns(new IdentityOptions());

            var securityStampOptions = new Mock<IOptions<SecurityStampValidatorOptions>>();
            securityStampOptions.Setup(a => a.Value).Returns(new SecurityStampValidatorOptions { ValidationInterval = TimeSpan.Zero });

            var claimsFactory = new UserClaimsPrincipalFactory<TestUser, TestRole>(manager.Object, roleManager.Object, options.Object);

            var loggerFactory = new MockLoggerFactory();
            var logger = loggerFactory.CreateLogger<SignInManager<TestUser>>();

            var helper = new Mock<SignInManager<TestUser>>(manager.Object, contextAccessor.Object, claimsFactory, options.Object, logger, new Mock<IAuthenticationSchemeProvider>().Object, new Mock<IUserConfirmation<TestUser>>().Object);
            var properties = new AuthenticationProperties { IssuedUtc = DateTimeOffset.UtcNow.AddSeconds(-1), IsPersistent = isPersistent };

            var id = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
            id.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            var principal = new ClaimsPrincipal(id);

            helper.Setup(s => s.ValidateSecurityStampAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user).Verifiable();
            helper.Setup(s => s.CreateUserPrincipalAsync(user)).ReturnsAsync(principal).Verifiable();

            var logFactory = new MockLoggerFactory();
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton(options.Object);
            services.AddSingleton(helper.Object);
            services.AddSingleton<ISecurityStampValidator>(new SecurityStampValidator<TestUser>(securityStampOptions.Object, helper.Object, new SystemClock(), loggerFactory));

            context.Setup(c => c.RequestServices).Returns(services.BuildServiceProvider());
            contextAccessor.Setup(a => a.HttpContext).Returns(context.Object);

            var ticket = new AuthenticationTicket(principal, 
                properties, 
                IdentityConstants.ApplicationScheme);
            var cookieContext = new CookieValidatePrincipalContext(context.Object, new AuthenticationSchemeBuilder(IdentityConstants.ApplicationScheme) { HandlerType = typeof(NoopHandler) }.Build(), new CookieAuthenticationOptions(), ticket);
            Assert.NotNull(cookieContext.Properties);
            Assert.NotNull(cookieContext.Options);
            Assert.NotNull(cookieContext.Principal);
            await
                SecurityStampValidator.ValidatePrincipalAsync(cookieContext);
            Assert.NotNull(cookieContext.Principal);
            helper.VerifyAll();
        }

        private static Mock<UserManager<TestUser>> SetupUserManager(TestUser user)
        {
            var manager = MockHelpers.MockUserManager<TestUser>();
            manager.Setup(m => m.FindByNameAsync(user.UserName)).ReturnsAsync(user);
            manager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            manager.Setup(m => m.GetUserIdAsync(user)).ReturnsAsync(user.Id.ToString());
            manager.Setup(m => m.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
            return manager;
        }

        [Fact]
        public async Task OnValidateIdentityRejectsWhenValidateSecurityStampFails()
        {
            var user = new TestUser { UserName = "test" };

            var manager = SetupUserManager(user);
            manager.Setup(m => m.SupportsUserLockout).Returns(true).Verifiable();
            manager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true).Verifiable();

            var context = new Mock<HttpContext>();

            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.HttpContext).Returns(context.Object);

            var roleManager = MockHelpers.MockRoleManager<TestRole>();

            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(a => a.Value).Returns(new IdentityOptions());

            var securityStampOptions = new Mock<IOptions<SecurityStampValidatorOptions>>();
            securityStampOptions.Setup(a => a.Value).Returns(new SecurityStampValidatorOptions { ValidationInterval = TimeSpan.Zero });

            var claimsFactory = new UserClaimsPrincipalFactory<TestUser, TestRole>(manager.Object, roleManager.Object, options.Object);
            var logStore = new StringBuilder();
            var loggerFactory = new MockLoggerFactory();
            var logger = loggerFactory.CreateLogger<SignInManager<TestUser>>();

            var helper = new Mock<SignInManager<TestUser>>(manager.Object, contextAccessor.Object, claimsFactory, options.Object, logger, new Mock<IAuthenticationSchemeProvider>().Object, new Mock<IUserConfirmation<TestUser>>().Object);
            var properties = new AuthenticationProperties { IssuedUtc = DateTimeOffset.UtcNow.AddSeconds(-1) };

            var id = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
            id.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            var principal = new ClaimsPrincipal(id);
            //because the request fails the create user principal is never called and therefore not verifiable
            //helper.Setup(s => s.CreateUserPrincipalAsync(user)).ReturnsAsync(principal).Verifiable();
            //helper.Setup(s => s.ValidateSecurityStampAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user).Verifiable();
            helper.Setup(s => s.ValidateSecurityStampAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(default(TestUser)).Verifiable();

            var services = new ServiceCollection();

            services.AddSingleton(options.Object);
            services.AddSingleton(helper.Object);
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<ISecurityStampValidator>(new SecurityStampValidator<TestUser>(securityStampOptions.Object, helper.Object, new SystemClock(), loggerFactory));

            context.Setup(c => c.RequestServices).Returns(services.BuildServiceProvider());
            contextAccessor.Setup(a => a.HttpContext).Returns(context.Object);

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(id),
                new AuthenticationProperties { IssuedUtc = DateTimeOffset.UtcNow.AddSeconds(-1) },
                IdentityConstants.ApplicationScheme);

            var cookieContext = new CookieValidatePrincipalContext(context.Object, new AuthenticationSchemeBuilder(IdentityConstants.ApplicationScheme) { HandlerType = typeof(NoopHandler) }.Build(), new CookieAuthenticationOptions(), ticket);

            Assert.NotNull(cookieContext.Properties);
            Assert.NotNull(cookieContext.Options);
            Assert.NotNull(cookieContext.Principal);

            await SecurityStampValidator.ValidatePrincipalAsync(cookieContext);

            Assert.Null(cookieContext.Principal);

            helper.VerifyAll();
        }

        [Fact]
        public async Task OnValidateIdentityRejectsWhenNoIssuedUtc()
        {
            var user = new TestUser { UserName = "test" };

            var manager = SetupUserManager(user);
            manager.Setup(m => m.SupportsUserLockout).Returns(true).Verifiable();
            manager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true).Verifiable();

            var context = new Mock<HttpContext>();

            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.HttpContext).Returns(context.Object);

            var roleManager = MockHelpers.MockRoleManager<TestRole>();

            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(a => a.Value).Returns(new IdentityOptions());

            var securityStampOptions = new Mock<IOptions<SecurityStampValidatorOptions>>();
            securityStampOptions.Setup(a => a.Value).Returns(new SecurityStampValidatorOptions { ValidationInterval = TimeSpan.Zero });

            var claimsFactory = new UserClaimsPrincipalFactory<TestUser, TestRole>(manager.Object, roleManager.Object, options.Object);
            var logStore = new StringBuilder();
            var loggerFactory = new MockLoggerFactory();
            var logger = loggerFactory.CreateLogger<SignInManager<TestUser>>();

            var helper = new Mock<SignInManager<TestUser>>(manager.Object, contextAccessor.Object, claimsFactory, options.Object, logger, new Mock<IAuthenticationSchemeProvider>().Object, new Mock<IUserConfirmation<TestUser>>().Object);
            var properties = new AuthenticationProperties { IssuedUtc = DateTimeOffset.UtcNow.AddSeconds(-1) };

            var id = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
            id.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            var principal = new ClaimsPrincipal(id);
            //because the request fails the create user principal is never called and therefore not verifiable
            //helper.Setup(s => s.CreateUserPrincipalAsync(user)).ReturnsAsync(principal).Verifiable();
            //helper.Setup(s => s.ValidateSecurityStampAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user).Verifiable();
            helper.Setup(s => s.ValidateSecurityStampAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(default(TestUser)).Verifiable();

            var services = new ServiceCollection();

            services.AddSingleton(options.Object);
            services.AddSingleton(helper.Object);
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<ISecurityStampValidator>(new SecurityStampValidator<TestUser>(securityStampOptions.Object, helper.Object, new SystemClock(), loggerFactory));

            context.Setup(c => c.RequestServices).Returns(services.BuildServiceProvider());
            contextAccessor.Setup(a => a.HttpContext).Returns(context.Object);

            // testing the ticket UTC setting, in this case lack of setting
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(id),
                new AuthenticationProperties(),
                IdentityConstants.ApplicationScheme);

            var cookieContext = new CookieValidatePrincipalContext(context.Object, new AuthenticationSchemeBuilder(IdentityConstants.ApplicationScheme) { HandlerType = typeof(NoopHandler) }.Build(), new CookieAuthenticationOptions(), ticket);

            Assert.NotNull(cookieContext.Properties);
            Assert.NotNull(cookieContext.Options);
            Assert.NotNull(cookieContext.Principal);

            await SecurityStampValidator.ValidatePrincipalAsync(cookieContext);

            Assert.Null(cookieContext.Principal);

            helper.VerifyAll();
        }

        [Fact]
        public async Task OnValidateIdentityDoesNotRejectsWhenNotExpired()
        {
            var user = new TestUser { UserName = "test" };

            var manager = SetupUserManager(user);
            manager.Setup(m => m.SupportsUserLockout).Returns(true).Verifiable();
            manager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true).Verifiable();

            var context = new Mock<HttpContext>();

            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.HttpContext).Returns(context.Object);

            var roleManager = MockHelpers.MockRoleManager<TestRole>();

            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(a => a.Value).Returns(new IdentityOptions());

            var securityStampOptions = new Mock<IOptions<SecurityStampValidatorOptions>>();
            securityStampOptions.Setup(a => a.Value).Returns(new SecurityStampValidatorOptions { ValidationInterval = TimeSpan.Zero });

            var claimsFactory = new UserClaimsPrincipalFactory<TestUser, TestRole>(manager.Object, roleManager.Object, options.Object);
            var logStore = new StringBuilder();
            var loggerFactory = new MockLoggerFactory();
            var logger = loggerFactory.CreateLogger<SignInManager<TestUser>>();

            var helper = new Mock<SignInManager<TestUser>>(manager.Object, contextAccessor.Object, claimsFactory, options.Object, logger, new Mock<IAuthenticationSchemeProvider>().Object, new Mock<IUserConfirmation<TestUser>>().Object);
            var properties = new AuthenticationProperties { IssuedUtc = DateTimeOffset.UtcNow.AddSeconds(-1) };

            var id = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
            id.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            var principal = new ClaimsPrincipal(id);
            //because the request fails the create user principal is never called and therefore not verifiable
            //helper.Setup(s => s.CreateUserPrincipalAsync(user)).ReturnsAsync(principal).Verifiable();
            //helper.Setup(s => s.ValidateSecurityStampAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user).Verifiable();
            //helper.Setup(s => s.ValidateSecurityStampAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(default(TestUser)).Verifiable();
            helper.Setup(s => s.SignInAsync(user, false, null)).Throws(new Exception("Shouldn't be called"));

            var services = new ServiceCollection();

            services.AddSingleton(options.Object);
            services.AddSingleton(helper.Object);
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<ISecurityStampValidator>(new SecurityStampValidator<TestUser>(securityStampOptions.Object, helper.Object, new SystemClock(), loggerFactory));

            context.Setup(c => c.RequestServices).Returns(services.BuildServiceProvider());
            contextAccessor.Setup(a => a.HttpContext).Returns(context.Object);

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(id),
                new AuthenticationProperties { IssuedUtc = DateTimeOffset.UtcNow },
                IdentityConstants.ApplicationScheme);

            var cookieContext = new CookieValidatePrincipalContext(context.Object, new AuthenticationSchemeBuilder(IdentityConstants.ApplicationScheme) { HandlerType = typeof(NoopHandler) }.Build(), new CookieAuthenticationOptions(), ticket);
            Assert.NotNull(cookieContext.Properties);
            Assert.NotNull(cookieContext.Options);
            Assert.NotNull(cookieContext.Principal);
            await SecurityStampValidator.ValidatePrincipalAsync(cookieContext);
            Assert.NotNull(cookieContext.Principal);
        }
    }
}