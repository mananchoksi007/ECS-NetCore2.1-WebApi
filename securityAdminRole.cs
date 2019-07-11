using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ECSApi
{
    public class securityAdminRole
    {
        public string SecurityAdminkey { get; set; }

        [JsonProperty("PartnerType")]
        public string PartnerType { get; set; }


        [JsonProperty("AdminRole")]
        public string AdminRole { get; set; }
    }
}
