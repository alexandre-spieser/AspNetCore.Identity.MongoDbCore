using MongoDB.Bson;
using System.ComponentModel;

namespace AspNetCore.Identity.MongoDbCore.Extensions
{
    /// <summary>
    /// A set of extensions for string.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the provided <paramref name="id"/> to a strongly typed key object.        
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TKey ToTKey<TKey>(this string id)
        {
            if (id == null)
            {
                return default(TKey);
            }
            var typeOfKey = typeof(TKey);
            if (typeOfKey.Name != "ObjectId")
            {
                return (TKey)TypeDescriptor.GetConverter(typeOfKey).ConvertFromInvariantString(id);
            }
            return (TKey)(object)(new ObjectId(id));
        }
    }
}
