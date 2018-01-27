// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AspNetCore.Identity.MongoDbCore.Models;
using AspNetCore.Identity.MongoDbCore.IntegrationTests.Infrastructure;
using MongoDbGenericRepository;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.MongoDbCore.Test
{
    public class UserStoreTest : IdentitySpecificationTestBase<MongoDbIdentityUser, MongoDbIdentityRole>, IClassFixture<MongoDatabaseFixture<MongoDbIdentityUser, MongoDbIdentityRole, string>>
    {
        private readonly MongoDatabaseFixture<MongoDbIdentityUser, MongoDbIdentityRole, string> _fixture;

        public UserStoreTest(MongoDatabaseFixture<MongoDbIdentityUser, MongoDbIdentityRole, string> fixture)
        {
            _fixture = fixture;
        }

        protected override bool ShouldSkipDbTests()
            => false;

        [Fact]
        public void CanCreateUserUsingMongoDB()
        {
            var user = CreateTestUser();
            user.Id = Guid.NewGuid().ToString();
            var guidString = user.Id.ToString();
            user.UserName = guidString;
            Container.MongoRepository.AddOne<MongoDbIdentityUser, string>(user);
            Assert.True(Container.MongoRepository.Any<MongoDbIdentityUser, string>(u => u.UserName == guidString));
            Assert.NotNull(Container.MongoRepository.GetOne<MongoDbIdentityUser, string>(u => u.UserName == guidString));
        }

        protected override void AddUserStore(IServiceCollection services, object context = null)
        {
            services.AddSingleton<IUserStore<MongoDbIdentityUser>>(new MongoUserStore<MongoDbIdentityUser, MongoDbIdentityRole, IMongoDbContext, string>(Container.MongoRepository.Context));
        }

        protected override void AddRoleStore(IServiceCollection services, object context = null)
        {
            services.AddSingleton<IRoleStore<MongoDbIdentityRole>>(new MongoRoleStore<MongoDbIdentityRole, IMongoDbContext>(Container.MongoRepository.Context));
        }

        [Fact]
        public async Task SqlUserStoreMethodsThrowWhenDisposedTest()
        {
            var store = new MongoUserStore(Container.MongoRepository.Context);
            store.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.AddClaimsAsync(null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.AddLoginAsync(null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.AddToRoleAsync(null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.GetClaimsAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.GetLoginsAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.GetRolesAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.IsInRoleAsync(null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.RemoveClaimsAsync(null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.RemoveLoginAsync(null, null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await store.RemoveFromRoleAsync(null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.RemoveClaimsAsync(null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.ReplaceClaimAsync(null, null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.FindByLoginAsync(null, null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.FindByIdAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.FindByNameAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.CreateAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.UpdateAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.DeleteAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await store.SetEmailConfirmedAsync(null, true));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await store.GetEmailConfirmedAsync(null));
            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await store.SetPhoneNumberConfirmedAsync(null, true));
            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await store.GetPhoneNumberConfirmedAsync(null));
        }

        [Fact]
        public async Task UserStorePublicNullCheckTest()
        {
            Assert.Throws<ArgumentNullException>("context", () => new MongoUserStore(null));
            var store = new MongoUserStore(Container.MongoRepository.Context);
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetUserIdAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetUserNameAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.SetUserNameAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.CreateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.UpdateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.DeleteAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.AddClaimsAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.ReplaceClaimAsync(null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.RemoveClaimsAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetClaimsAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetLoginsAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetRolesAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.AddLoginAsync(null, null));
            await
                Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.RemoveLoginAsync(null, null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.AddToRoleAsync(null, null));
            await
                Assert.ThrowsAsync<ArgumentNullException>("user",
                    async () => await store.RemoveFromRoleAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.IsInRoleAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetPasswordHashAsync(null));
            await
                Assert.ThrowsAsync<ArgumentNullException>("user",
                    async () => await store.SetPasswordHashAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetSecurityStampAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user",
                async () => await store.SetSecurityStampAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("login", async () => await store.AddLoginAsync(new MongoDbIdentityUser("fake"), null));
            await Assert.ThrowsAsync<ArgumentNullException>("claims",
                async () => await store.AddClaimsAsync(new MongoDbIdentityUser("fake"), null));
            await Assert.ThrowsAsync<ArgumentNullException>("claims",
                async () => await store.RemoveClaimsAsync(new MongoDbIdentityUser("fake"), null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetEmailConfirmedAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user",
                async () => await store.SetEmailConfirmedAsync(null, true));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetEmailAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.SetEmailAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetPhoneNumberAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.SetPhoneNumberAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>("user",
                async () => await store.GetPhoneNumberConfirmedAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user",
                async () => await store.SetPhoneNumberConfirmedAsync(null, true));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetTwoFactorEnabledAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user",
                async () => await store.SetTwoFactorEnabledAsync(null, true));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetAccessFailedCountAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetLockoutEnabledAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.SetLockoutEnabledAsync(null, false));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.GetLockoutEndDateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.SetLockoutEndDateAsync(null, new DateTimeOffset()));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.ResetAccessFailedCountAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await store.IncrementAccessFailedCountAsync(null));
            await Assert.ThrowsAsync<ArgumentException>("normalizedRoleName", async () => await store.AddToRoleAsync(new MongoDbIdentityUser("fake"), null));
            await Assert.ThrowsAsync<ArgumentException>("normalizedRoleName", async () => await store.RemoveFromRoleAsync(new MongoDbIdentityUser("fake"), null));
            await Assert.ThrowsAsync<ArgumentException>("normalizedRoleName", async () => await store.IsInRoleAsync(new MongoDbIdentityUser("fake"), null));
            await Assert.ThrowsAsync<ArgumentException>("normalizedRoleName", async () => await store.AddToRoleAsync(new MongoDbIdentityUser("fake"), ""));
            await Assert.ThrowsAsync<ArgumentException>("normalizedRoleName", async () => await store.RemoveFromRoleAsync(new MongoDbIdentityUser("fake"), ""));
            await Assert.ThrowsAsync<ArgumentException>("normalizedRoleName", async () => await store.IsInRoleAsync(new MongoDbIdentityUser("fake"), ""));
        }

        [Fact]
        public async Task CanCreateUsingManager()
        {
            var manager = CreateManager();
            var user = CreateTestUser();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(user));
            IdentityResultAssert.IsSuccess(await manager.DeleteAsync(user));
        }

        [Fact]
        public async Task TwoUsersSamePasswordDifferentHash()
        {
            var manager = CreateManager();
            var userA = CreateTestUser();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(userA, "password"));
            var userB = CreateTestUser();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(userB, "password"));

            Assert.NotEqual(userA.PasswordHash, userB.PasswordHash);
        }

        [Fact]
        public async Task AddUserToUnknownRoleFails()
        {
            var manager = CreateManager();
            var u = CreateTestUser();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(u));
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await manager.AddToRoleAsync(u, "bogus"));
        }

        [Fact]
        public async Task ConcurrentUpdatesWillFail()
        {
            var user = CreateTestUser();
            var manager = CreateManager();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(user));
            var manager1 = CreateManager();
            var manager2 = CreateManager();
            var user1 = await manager1.FindByIdAsync(user.Id);
            var user2 = await manager2.FindByIdAsync(user.Id);
            Assert.NotNull(user1);
            Assert.NotNull(user2);
            Assert.NotSame(user1, user2);
            user1.UserName = Guid.NewGuid().ToString();
            user2.UserName = Guid.NewGuid().ToString();
            IdentityResultAssert.IsSuccess(await manager1.UpdateAsync(user1));
            IdentityResultAssert.IsFailure(await manager2.UpdateAsync(user2), new IdentityErrorDescriber().ConcurrencyFailure());
        }

        [Fact]
        public async Task ConcurrentUpdatesWillFailWithDetachedUser()
        {
            var user = CreateTestUser();
            var manager = CreateManager();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(user));
            var manager1 = CreateManager();
            var manager2 = CreateManager();
            var user2 = await manager2.FindByIdAsync(user.Id);
            Assert.NotNull(user2);
            Assert.NotSame(user, user2);
            user.UserName = Guid.NewGuid().ToString();
            user2.UserName = Guid.NewGuid().ToString();
            IdentityResultAssert.IsSuccess(await manager1.UpdateAsync(user));
            IdentityResultAssert.IsFailure(await manager2.UpdateAsync(user2), new IdentityErrorDescriber().ConcurrencyFailure());
        }


        [Fact]
        public async Task DeleteAModifiedUserWillFail()
        {
            var user = CreateTestUser();

            var manager = CreateManager();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(user));

            var manager1 = CreateManager();
            var manager2 = CreateManager();
            var user1 = await manager1.FindByIdAsync(user.Id);
            var user2 = await manager2.FindByIdAsync(user.Id);
            Assert.NotNull(user1);
            Assert.NotNull(user2);
            Assert.NotSame(user1, user2);
            user1.UserName = Guid.NewGuid().ToString();
            IdentityResultAssert.IsSuccess(await manager1.UpdateAsync(user1));
            IdentityResultAssert.IsFailure(await manager2.DeleteAsync(user2), new IdentityErrorDescriber().ConcurrencyFailure());
        }

        [Fact]
        public async Task ConcurrentRoleUpdatesWillFail()
        {
            var role = CreateRole();

            var manager = CreateRoleManager();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(role));

            var manager1 = CreateRoleManager();
            var manager2 = CreateRoleManager();
            var role1 = await manager1.FindByIdAsync(role.Id);
            var role2 = await manager2.FindByIdAsync(role.Id);
            Assert.NotNull(role1);
            Assert.NotNull(role2);
            Assert.NotSame(role1, role2);
            role1.Name = Guid.NewGuid().ToString();
            role2.Name = Guid.NewGuid().ToString();
            IdentityResultAssert.IsSuccess(await manager1.UpdateAsync(role1));
            IdentityResultAssert.IsFailure(await manager2.UpdateAsync(role2), new IdentityErrorDescriber().ConcurrencyFailure());
        }

        [Fact]
        public async Task ConcurrentRoleUpdatesWillFailWithDetachedRole()
        {
            var role = CreateRole();
            var manager = CreateRoleManager();
            IdentityResultAssert.IsSuccess(await manager.CreateAsync(role));
            var manager1 = CreateRoleManager();
            var manager2 = CreateRoleManager();
            var role2 = await manager2.FindByIdAsync(role.Id);
            Assert.NotNull(role);
            Assert.NotNull(role2);
            Assert.NotSame(role, role2);
            role.Name = Guid.NewGuid().ToString();
            role2.Name = Guid.NewGuid().ToString();
            IdentityResultAssert.IsSuccess(await manager1.UpdateAsync(role));
            IdentityResultAssert.IsFailure(await manager2.UpdateAsync(role2), new IdentityErrorDescriber().ConcurrencyFailure());
        }

        private MongoDbIdentityRole CreateRole()
        {
            var guid = Guid.NewGuid().ToString();
            var role = new MongoDbIdentityRole(guid);
            _fixture.RolesToDelete.Add(role);
            return role;
        }

        [Fact]
        public async Task DeleteAModifiedRoleWillFail()
        {
            var role = CreateRole();
            var manager = CreateRoleManager();
            var result = await manager.CreateAsync(role);

            IdentityResultAssert.IsSuccess(result);

            var manager1 = CreateRoleManager();
            var manager2 = CreateRoleManager(true);
            var role1 = await manager1.FindByIdAsync(role.Id);
            var role2 = await manager2.FindByIdAsync(role.Id);
            Assert.NotNull(role1);
            Assert.NotNull(role2);
            Assert.NotSame(role1, role2);
            role1.Name = Guid.NewGuid().ToString();
            IdentityResultAssert.IsSuccess(await manager1.UpdateAsync(role1));
            IdentityResultAssert.IsFailure(await manager2.DeleteAsync(role2), new IdentityErrorDescriber().ConcurrencyFailure());
        }

        protected override MongoDbIdentityUser CreateTestUser(string namePrefix = "", string email = "", string phoneNumber = "",
            bool lockoutEnabled = false, DateTimeOffset? lockoutEnd = default(DateTimeOffset?), bool useNamePrefixAsUserName = false)
        {
            var user = new MongoDbIdentityUser
            {
                UserName = useNamePrefixAsUserName ? namePrefix : string.Format("{0}{1}", namePrefix, Guid.NewGuid()),
                Email = email,
                PhoneNumber = phoneNumber,
                LockoutEnabled = lockoutEnabled,
                LockoutEnd = lockoutEnd
            };
            _fixture.UsersToDelete.Add(user);
            return user;
        }

        protected override MongoDbIdentityRole CreateTestRole(string roleNamePrefix = "", bool useRoleNamePrefixAsRoleName = false)
        {
            var roleName = useRoleNamePrefixAsRoleName ? roleNamePrefix : string.Format("{0}{1}", roleNamePrefix, Guid.NewGuid());
            var role = new MongoDbIdentityRole(roleName);
            _fixture.RolesToDelete.Add(role);
            return role;
        }

        protected override void SetUserPasswordHash(MongoDbIdentityUser user, string hashedPassword)
        {
            user.PasswordHash = hashedPassword;
        }

        protected override Expression<Func<MongoDbIdentityUser, bool>> UserNameEqualsPredicate(string userName) => u => u.UserName == userName;

        protected override Expression<Func<MongoDbIdentityRole, bool>> RoleNameEqualsPredicate(string roleName) => r => r.Name == roleName;

        protected override Expression<Func<MongoDbIdentityRole, bool>> RoleNameStartsWithPredicate(string roleName) => r => r.Name.StartsWith(roleName);

        protected override Expression<Func<MongoDbIdentityUser, bool>> UserNameStartsWithPredicate(string userName) => u => u.UserName.StartsWith(userName);

    }

    public class ApplicationUser : MongoIdentityUser { }
}
