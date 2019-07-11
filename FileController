using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static ECSApi.ECSUtils;

namespace ECSApi.Controllers
{
    [Route("api/[action]")]
    [ApiController]
    public class FileController : Controller
    {
        IECSUtils ecsUtils;   // Our Interface to S3 Storage

        public FileController(IECSUtils _ecsutils)
        {
            ecsUtils = _ecsutils;
        }
       

        [HttpPost]
        public async void AddSecurityAdminRole([FromBody]SecurityAdminRole value)
        {

            string retValue = string.Empty;
            try
            {
                Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(value.SecurityAdminRoleJson);
                securityAdminRole _data = new securityAdminRole();
                foreach (var data in values)
                {
                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();
                    _data.SecurityAdminkey = g.ToString();

                    if (data.Key.ToString().Equals("PartnerType"))
                    {
                        _data.PartnerType = data.Value;
                    }
                    if (data.Key.ToString().Equals("AdminRole"))
                    {
                        _data.AdminRole = data.Value;
                    }
                }
                await ecsUtils.WriteSecurityAdminRoleToBucket(_data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //Customer c = new Customer();
            ////c.firstName = value.firstName;
            ////c.lastName = value.lastName;
            ////c.customerNumber = value.customerNumber;
            //c.firstName = "Manan";
            //c.lastName = "Choksi";
            //c.customerNumber = 1232;
            //// customerList.Add(c);            // This adds to the List being maintained.  NO Longer Required since we will send it to
            // S3

        }


        [HttpGet]
        public async Task<ActionResult<List<securityAdminRole>>> GetSecurityAdminRole()
        {
            List<securityAdminRole> cList = await ecsUtils.ReadSecurityAdminRoleFromBucket();
            return cList;
            // return customerList; // This convers the list first into a JSON and returns the data.
        }
    }
    public class SecurityAdminRole
    {
        public string SecurityAdminRoleJson { get; set; }
    }

    public static class Helper
    {

    }
}
