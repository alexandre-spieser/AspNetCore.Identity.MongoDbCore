using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Threading;

namespace AspNetCore.Identity.MongoDbCore
{
    internal static class MongoConfig
    {
        private static bool _initialized = false;
        private static object _initializationLock = new object();
        private static object _initializationTarget;

        public static void EnsureConfigured()
        {
            EnsureConfiguredImpl();
        }

        private static void EnsureConfiguredImpl()
        {
            LazyInitializer.EnsureInitialized(ref _initializationTarget, ref _initialized, ref _initializationLock, () =>
            {
                Configure();
                return null;
            });
        }

        private static void Configure()
        {
            RegisterConventions();
        }

        private static void RegisterConventions()
        {
            var pack = new ConventionPack
            {
                new IgnoreIfNullConvention(false),
                new CamelCaseElementNameConvention(),
            };

            ConventionRegistry.Register("AspNetCore.Identity.MongoDbCore", pack, IsConventionApplicable);
        }

        private static bool IsConventionApplicable(Type type)
        {
            return type == typeof(MongoIdentityUser<>);
        }
    }
}