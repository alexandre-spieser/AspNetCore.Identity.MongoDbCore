namespace AspNetCore.Identity.MongoDbCore.Models
{
    /// <summary>
    /// A class representing the tokens a <see cref="MongoIdentityUser{TKey}"/> can have.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Gets or sets the LoginProvider this token is from.
        /// </summary>
        public string LoginProvider { get; set; }
        /// <summary>
        /// Gets or sets the name of the token. 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the token value.
        /// </summary>
        public string Value { get; set; }
    }
}
