using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MongoIdentitySample.Mvc.Models.ManageViewModels
{
    public class DisplayRecoveryCodesViewModel
    {
        [Required]
        public IEnumerable<string> Codes { get; set; }

    }
}
