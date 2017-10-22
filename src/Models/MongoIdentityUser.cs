using System;
using System.Collections.Generic;
using MongoDbGenericRepository.Models;
using System.Linq;
using MongoDB.Driver;
using AspNetCore.Identity.MongoDbCore.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.MongoDbCore.Models
{
    public class Token
    {
        /// <summary>
        /// Gets or sets the LoginProvider this token is from.
        /// </summary>
        public string LoginProvider { get; set; }
        /// <summary>
        /// Gets or sets the name of the token. 
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the token value.
        /// </summary>
        public string Value { get; set; }
    }

    public class MongoClaim {
        public string Type { get; set; }
        public string Value { get; set; }
        public string Issuer { get; set; }
    }

    public class UserRole
    {
        public object UserId { get; set; }
        public object RoleId { get; set; }
    }

    public class MongoDbIdentityUser : MongoIdentityUser<string>
    {
        public MongoDbIdentityUser() : base()
        {
        }

        public MongoDbIdentityUser(string userName) : base(userName)
        {
        }

        public MongoDbIdentityUser(string userName, string email) : base(userName, email)
        {
        }
    }

    public class MongoIdentityUser : MongoIdentityUser<Guid>
    {
        public MongoIdentityUser() : base()
        {
        }

        public MongoIdentityUser(string userName) : base(userName)
        {
        }

        public MongoIdentityUser(string userName, string email) : base(userName, email)
        {
        }
    }

    public class MongoIdentityUser<TKey> : IdentityUser<TKey>, IDocument<TKey>, IClaimHolder
        where TKey : IEquatable<TKey>
    {

        public int Version { get; set; }

        public DateTime CreatedOn { get; private set; }
        public DateTime? LockoutEndDate { get; private set; }
        public DateTime? DeletedOn { get; private set; }
        public List<MongoClaim> Claims { get; set; }
        public List<TKey> Roles { get; set; }
        public List<UserLoginInfo> Logins { get; set; }
        public List<Token> Tokens { get; set; }

        private void InitializeFields()
        {
            Claims = new List<MongoClaim>();
            Logins = new List<UserLoginInfo>();
            Roles = new List<TKey>();
            Tokens = new List<Token>();
            Guid guidValue = Guid.NewGuid();

            var idTypeName = typeof(TKey).Name;
            switch (idTypeName)
            {
                case "Guid":
                    Id = (TKey)(object)guidValue;
                    break;
                case "Int32":
                    Id = (TKey)(object)GlobalVariables.Random.Next(1, int.MaxValue);
                    break;
                case "String":
                    Id = (TKey)(object)guidValue.ToString();
                    break;
            }

        }

        public MongoIdentityUser()
        {
            CreatedOn = DateTime.UtcNow;
            SetVersion(1);
            InitializeFields();
        }

        public MongoIdentityUser(string userName, string email) : this(userName)
        {
            if (email != null)
            {
                Email = email.ToLowerInvariant().Trim();
            }
        }

        public MongoIdentityUser(string userName)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            CreatedOn = DateTime.UtcNow;

            SetVersion(1);
            InitializeFields();
            Roles = new List<TKey>();
        }

        public virtual MongoIdentityUser<TKey> SetId(TKey key)
        {
            Id = key;
            return this;
        }

        public virtual MongoIdentityUser<TKey> SetVersion(int version)
        {
            Version = 1;
            return this;
        }

        public virtual void EnableTwoFactorAuthentication()
        {
            TwoFactorEnabled = true;
        }

        public virtual void DisableTwoFactorAuthentication()
        {
            TwoFactorEnabled = false;
        }

        public virtual void EnableLockout()
        {
            LockoutEnabled = true;
        }

        public virtual void DisableLockout()
        {
            LockoutEnabled = false;
        }

        public virtual void SetEmail(string email)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
        }

        public virtual void SetNormalizedUserName(string normalizedUserName)
        {
            NormalizedUserName = normalizedUserName ?? throw new ArgumentNullException(nameof(normalizedUserName));
        }

        public virtual void SetPhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public virtual void SetPasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash;
        }

        public virtual void SetSecurityStamp(string securityStamp)
        {
            SecurityStamp = securityStamp;
        }

        public virtual void SetAccessFailedCount(int accessFailedCount)
        {
            AccessFailedCount = accessFailedCount;
        }

        public virtual void ResetAccessFailedCount()
        {
            AccessFailedCount = 0;
        }

        public virtual void LockUntil(DateTime lockoutEndDate)
        {
            LockoutEndDate = lockoutEndDate;
        }

        public void Delete()
        {
            if (DeletedOn != null)
            {
                throw new InvalidOperationException($"User '{Id}' has already been deleted.");
            }

            DeletedOn = DateTime.UtcNow;
        }

        #region Role Management

        public virtual IdentityUserRole<TKey> GetUserRole(TKey roleId)
        {
            var foundRoleId = Roles.FirstOrDefault(e => e.Equals(roleId));
            if (!foundRoleId.Equals(default(TKey)))
            {
                return new IdentityUserRole<TKey>
                {
                    UserId = Id,
                    RoleId = foundRoleId
                };
            }
            return default(IdentityUserRole<TKey>);
        }

        public virtual bool RemoveRole(TKey roleId)
        {
            var roleClaim = Roles.FirstOrDefault(e => e.Equals(roleId));
            if (!roleClaim.Equals(default(TKey)))
            {
                Roles.Remove(roleId);
                return true;
            }
            return false;
        }

        public virtual bool AddRole(TKey roleId)
        {
            if (!Roles.Contains(roleId))
            {
                Roles.Add(roleId);
                return true;
            }
            return false;
        }

        #endregion

        #region Login Management

        public virtual bool AddLogin(UserLoginInfo mongoUserLogin)
        {
            if (mongoUserLogin == null)
            {
                throw new ArgumentNullException(nameof(mongoUserLogin));
            }
            if (HasLogin(mongoUserLogin))
            {
                return false;
            }
            Logins.Add(mongoUserLogin);
            return true;
        }

        public virtual bool HasLogin(UserLoginInfo login)
        {
            return Logins.Any(e => e.LoginProvider == login.LoginProvider && e.ProviderKey == e.ProviderKey);
        }

        public virtual void RemoveLogin(UserLoginInfo mongoUserLogin)
        {
            if (mongoUserLogin == null)
            {
                throw new ArgumentNullException(nameof(mongoUserLogin));
            }

            Logins.Remove(mongoUserLogin);
        }

        public virtual IdentityUserLogin<TKey> GetUserLogin(string loginProvider, string providerKey)
        {

            var login = Logins.FirstOrDefault(e => e.LoginProvider == loginProvider && e.ProviderKey == providerKey);
            if (login != null)
            {
                return new IdentityUserLogin<TKey>
                {
                    UserId = Id,
                    LoginProvider = login.LoginProvider,
                    ProviderDisplayName = login.ProviderDisplayName,
                    ProviderKey = login.ProviderKey
                };
            }
            return default(IdentityUserLogin<TKey>);
        }

        #endregion

        #region Token Management


        /// <summary>
        /// Replaces a claim on a claim holder, implementing <see cref="IClaimHolder"/>.
        /// </summary>
        /// <param name="claimHolder">The object holding claims.</param>
        /// <param name="claim">The claim you want to replace.</param>
        /// <param name="newClaim">The new claim you want to set.</param>
        /// <returns>Returns true if the claim was replaced.</returns>
        public bool SetToken(IdentityUserToken<TKey> tokenToset, string value)
        {
            var token = Tokens.FirstOrDefault(e => e.LoginProvider == tokenToset.LoginProvider && e.Name == tokenToset.Name);
            if (token != null)
            {
                token.Value = value;
                return true;
            }
            return false;
        }

        public IdentityUserToken<TKey> GetToken(string loginProvider, string name)
        {
            var token = Tokens.FirstOrDefault(e => e.LoginProvider == loginProvider && e.Name == name);
            if (token != null)
            {
                return new IdentityUserToken<TKey>
                {
                    UserId = Id,
                    LoginProvider = token.LoginProvider,
                    Name = token.Name,
                    Value = token.Value
                };
            }
            return default(IdentityUserToken<TKey>);
        }

        public bool HasToken(IdentityUserToken<TKey> token)
        {
            return Tokens.Any(e => e.LoginProvider == token.LoginProvider
                                && e.Name == token.Name
                                && e.Value == token.Value);
        }

        public bool AddOrSet(IdentityUserToken<TKey> token)
        {
            var exists = GetToken(token.LoginProvider, token.Name);
            if (exists != null && exists.Value != token.Value)
            {
                return SetToken(exists, token.Value);
            }
            if (exists == null)
            {
                Tokens.Add(new Token
                {
                    LoginProvider = token.LoginProvider,
                    Name = token.Name,
                    Value = token.Value
                });
                return true;
            }
            return false;
        }

        public bool AddUserToken<TUserToken>(TUserToken token) where TUserToken : IdentityUserToken<TKey>
        {
            if (HasToken(token))
            {
                return false;
            }

            Tokens.Add(new Token
            {
                LoginProvider = token.LoginProvider,
                Name = token.Name,
                Value = token.Value
            });
            return true;
        }

        public bool RemoveUserToken<TUserToken>(TUserToken token) where TUserToken : IdentityUserToken<TKey>
        {
            var exists = Tokens.FirstOrDefault(e => e.LoginProvider == token.LoginProvider && e.Name == token.Name);
            if (exists == null)
            {
                return false;
            }
            Tokens.Remove(exists);
            return true;
        }

        #endregion Token Management



    }
}