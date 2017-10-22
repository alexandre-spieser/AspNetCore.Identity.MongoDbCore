using AspNetCore.Identity.MongoDbCore.Models;
using System.Collections.Generic;

namespace AspNetCore.Identity.MongoDbCore.Interfaces
{
    /// <summary>
    /// The interface for an object that holds claims.
    /// </summary>
    public interface IClaimHolder
    {
        List<MongoClaim> Claims { get; set; }
    }
}
