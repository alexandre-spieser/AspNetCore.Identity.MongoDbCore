namespace AspNetCore.Identity.MongoDbCore.Models
{
    /// <summary>
    /// A class representing the claims a <see cref="MongoIdentityUser{TKey}"/> can have.
    /// </summary>
    public class MongoClaim
    {
        /// <summary>
        /// The type of the claim.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The value of the claim.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// The issuer of the claim.
        /// </summary>
        public string Issuer { get; set; }
    }
}
