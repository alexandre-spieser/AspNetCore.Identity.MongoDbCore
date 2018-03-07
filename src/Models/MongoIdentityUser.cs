using System;
using System.Collections.Generic;
using MongoDbGenericRepository.Models;
using System.Linq;
using MongoDB.Driver;
using AspNetCore.Identity.MongoDbCore.Interfaces;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.MongoDbCore.Extensions;
using MongoDbGenericRepository.Utils;

namespace AspNetCore.Identity.MongoDbCore.Models
{

    /// <summary>
    /// A <see cref="MongoIdentityUser{TKey}"/> where TKey is a <see cref="string"/>
    /// </summary>
    public class MongoDbIdentityUser : MongoIdentityUser<string>
    {
        /// <summary>
        /// The constructor for a <see cref="MongoDbIdentityUser"/>
        /// </summary>
        public MongoDbIdentityUser() : base()
        {
        }

        /// <summary>
        /// The constructor for a <see cref="MongoDbIdentityUser"/>, taking a username.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        public MongoDbIdentityUser(string userName) : base(userName)
        {
        }

        /// <summary>
        /// The constructor for a <see cref="MongoDbIdentityUser"/>, taking a username and an email address.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="email">The email address of the user.</param>
        public MongoDbIdentityUser(string userName, string email) : base(userName, email)
        {
        }
    }

    /// <summary>
    /// A <see cref="MongoIdentityUser{TKey}"/> where TKey is a <see cref="Guid"/>
    /// </summary>
    public class MongoIdentityUser : MongoIdentityUser<Guid>
    {
        /// <summary>
        /// The constructor for a <see cref="MongoIdentityUser"/>
        /// </summary>
        public MongoIdentityUser() : base()
        {
        }

        /// <summary>
        /// The constructor for a <see cref="MongoDbIdentityUser"/>, taking a username.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        public MongoIdentityUser(string userName) : base(userName)
        {
        }

        /// <summary>
        /// The constructor for a <see cref="MongoDbIdentityUser"/>, taking a username and an email address.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="email">The email address of the user.</param>
        public MongoIdentityUser(string userName, string email) : base(userName, email)
        {
        }
    }

    /// <summary>
    /// A document representing an <see cref="IdentityUser{TKey}"/> document.
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
     public class MongoIdentityUser<TKey> : IdentityUser<TKey>, IDocument<TKey>, IClaimHolder
        where TKey : IEquatable<TKey>
    {

        /// <summary>
        /// The version of the schema do the <see cref="MongoIdentityUser{TKey}"/> document.
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// The date and time at which this user was created, in UTC.
        /// </summary>
        public DateTime CreatedOn { get; private set; }
        /// <summary>
        /// The claims this user has.
        /// </summary>
        public List<MongoClaim> Claims { get; set; }
        /// <summary>
        /// The role Ids of the roles that this user has.
        /// </summary>
        public List<TKey> Roles { get; set; }
        /// <summary>
        /// The list of <see cref="UserLoginInfo"/>s that this user has.
        /// </summary>
        public List<UserLoginInfo> Logins { get; set; }
        /// <summary>
        /// The list of <see cref="Token"/>s that this user has.
        /// </summary>
        public List<Token> Tokens { get; set; }

        /// <summary>
        /// The constructor for a <see cref="MongoIdentityUser{TKey}"/>, taking a username and an email address.
        /// </summary>
        public MongoIdentityUser()
        {
            SetVersion(1);
            InitializeFields();
        }

        /// <summary>
        /// The constructor for a <see cref="MongoIdentityUser{TKey}"/>, taking a username and an email address.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="email">The email address of the user.</param>
        public MongoIdentityUser(string userName, string email) : this(userName)
        {
            if (email != null)
            {
                Email = email.ToLowerInvariant().Trim();
            }
        }

        /// <summary>
        /// The constructor for a <see cref="MongoIdentityUser{TKey}"/>, taking a username.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        public MongoIdentityUser(string userName)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            SetVersion(1);
            InitializeFields();
        }

        /// <summary>
        /// Initialize the field of the MongoIdentityUser
        /// </summary>
        protected virtual void InitializeFields()
        {
            CreatedOn = DateTime.UtcNow;
            Claims = new List<MongoClaim>();
            Logins = new List<UserLoginInfo>();
            Roles = new List<TKey>();
            Tokens = new List<Token>();
            Id = IdGenerator.GetId<TKey>();
        }

        /// <summary>
        /// Sets the version of the schema for the <see cref="MongoIdentityUser{TKey}"/> document.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public virtual MongoIdentityUser<TKey> SetVersion(int version)
        {
            Version = 1;
            return this;
        }

        #region Role Management

        /// <summary>
        /// Removes a role.
        /// </summary>
        /// <param name="roleId">The Id of the role you want to remove.</param>
        /// <returns>True if the removal was successful.</returns>
        public virtual bool RemoveRole(TKey roleId)
        {
            var roleClaim = Roles.FirstOrDefault(e => e.Equals(roleId));
            if (roleClaim != null && !roleClaim.Equals(default(TKey)))
            {
                Roles.Remove(roleId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add a role to the user.
        /// </summary>
        /// <param name="roleId">The Id of the role you want to add.</param>
        /// <returns>True if the addition was successful.</returns>
        public virtual bool AddRole(TKey roleId)
        {
            if(roleId == null || roleId.Equals(default(TKey)))
            {
                throw new ArgumentNullException(nameof(roleId));
            }
            if (!Roles.Contains(roleId))
            {
                Roles.Add(roleId);
                return true;
            }
            return false;
        }

        #endregion

        #region Login Management

        /// <summary>
        /// Adds a user login to the user.
        /// </summary>
        /// <param name="userLoginInfo">The <see cref="UserLoginInfo"/> you want to add.</param>
        /// <returns>True if the addition was successful.</returns>
        public virtual bool AddLogin(UserLoginInfo userLoginInfo)
        {
            if (userLoginInfo == null)
            {
                throw new ArgumentNullException(nameof(userLoginInfo));
            }
            if (HasLogin(userLoginInfo))
            {
                return false;
            }
            
            Logins.Add(new UserLoginInfo(userLoginInfo.LoginProvider, userLoginInfo.ProviderKey, userLoginInfo.ProviderDisplayName));
            return true;
        }

        /// <summary>
        /// Checks if the user has the given <see cref="UserLoginInfo"/>.
        /// </summary>
        /// <param name="userLoginInfo">The <see cref="UserLoginInfo"/> we are looking for.</param>
        /// <returns>True if the user has the given <see cref="UserLoginInfo"/>.</returns>
        public virtual bool HasLogin(UserLoginInfo userLoginInfo)
        {
            return Logins.Any(e => e.LoginProvider == userLoginInfo.LoginProvider && e.ProviderKey == e.ProviderKey);
        }

        /// <summary>
        /// Removes a <see cref="UserLoginInfo"/> from the user.
        /// </summary>
        /// <param name="userLoginInfo"></param>
        public virtual bool RemoveLogin(UserLoginInfo userLoginInfo)
        {
            if (userLoginInfo == null)
            {
                throw new ArgumentNullException(nameof(userLoginInfo));
            }
            var loginToremove = Logins.FirstOrDefault(e => e.LoginProvider == userLoginInfo.LoginProvider && e.ProviderKey == e.ProviderKey);
            if (loginToremove != null)
            {
                Logins.Remove(loginToremove);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginProvider"></param>
        /// <param name="providerKey"></param>
        /// <returns></returns>
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
        /// Sets the token to a new value.
        /// </summary>
        /// <param name="tokenToset">The token you want to set you want to set.</param>
        /// <param name="value">The value you want to set the token to.</param>
        /// <returns>Returns true if the token was successfully set.</returns>
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

        /// <summary>
        /// Gets a token given the login provider and the name.
        /// </summary>
        /// <param name="loginProvider">The value for the login provider.</param>
        /// <param name="name">The name of the token.</param>
        /// <returns>An <see cref="IdentityUser{TKey}"/> if found, or null.</returns>
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

        /// <summary>
        /// Checks if a user has the given token.
        /// </summary>
        /// <param name="token">The token you are looking for.</param>
        /// <returns>True if the user has the given token</returns>
        public bool HasToken(IdentityUserToken<TKey> token)
        {
            return Tokens.Any(e => e.LoginProvider == token.LoginProvider
                                && e.Name == token.Name
                                && e.Value == token.Value);
        }

        /// <summary>
        /// Adds a token to the user.
        /// </summary>
        /// <typeparam name="TUserToken">The type of the token.</typeparam>
        /// <param name="token">The token you want to add.</param>
        /// <returns>True if the addition was successful.</returns>
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

        /// <summary>
        /// Removes a token from the user.
        /// </summary>
        /// <typeparam name="TUserToken">The type of the token.</typeparam>
        /// <param name="token">The token you want to remove.</param>
        /// <returns>True if the removal was successful.</returns>
        public bool RemoveUserToken<TUserToken>(TUserToken token) where TUserToken : IdentityUserToken<TKey>
        {
            var exists = Tokens.FirstOrDefault(e => e.LoginProvider == token.LoginProvider 
                                                 && e.Name == token.Name);
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