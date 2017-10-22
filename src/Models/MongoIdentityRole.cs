using AspNetCore.Identity.MongoDbCore.Interfaces;
using Microsoft.AspNetCore.Identity;
using MongoDbGenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace AspNetCore.Identity.MongoDbCore.Models
{
    public class MongoDbIdentityRole : MongoIdentityRole<string>
    {
        public MongoDbIdentityRole() : base()
        {
        }

        public MongoDbIdentityRole(string roleName) : base(roleName)
        {
        }
    }

    public class MongoIdentityRole : MongoIdentityRole<Guid>
    {
        public MongoIdentityRole() : base()
        {
        }

        public MongoIdentityRole(string roleName) : base(roleName)
        {
        }
    }

    public class MongoIdentityRole<TKey> : IdentityRole<TKey>, IDocument<TKey>, IClaimHolder
        where TKey : IEquatable<TKey>
    {

        private void InitializeFields()
        {
            Version = 1;
            Claims = new List<MongoClaim>();
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

        public MongoIdentityRole()
        {
            InitializeFields();
        }

        public MongoIdentityRole(string roleName)
        {
            Name = roleName;
            InitializeFields();
        }

        public MongoIdentityRole(string name, TKey key)
        {
            InitializeFields();
            Id = key;
            Name = Name;
        }

        /// <summary>
        /// The version of the role schema
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The claims associated to the role
        /// </summary>
        public List<MongoClaim> Claims { get; set; }
    }
}
