using AspNetCore.Identity.MongoDbCore.Models;
using System.Collections.Generic;

namespace AspNetCore.Identity.MongoDbCore.Interfaces
{
    /// <summary>
    /// The interface for an object that holds claims.
    /// </summary>
    public interface IClaimHolder
    {
        /// <summary>
        /// The claims the <see cref="IClaimHolder"/> has.
        /// </summary>
        List<MongoClaim> Claims { get; set; }
    }
}
