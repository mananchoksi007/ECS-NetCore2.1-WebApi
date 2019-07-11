using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Amazon.S3;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Microsoft.AspNetCore.Hosting;

namespace ECSApi
{

    public interface IECSUtils
    {

        Task<List<securityAdminRole>> ReadSecurityAdminRoleFromBucket();
        Task<PutObjectResponse> WriteSecurityAdminRoleToBucket(securityAdminRole g);

    }

    public class ECSUtils : IECSUtils    // Implements the two methods. It requires the 4 items which it reads from config...
    {
        string s3AccessKey = null;
        string s3SecretKey = null;
        string s3EndPoint = null;
        string SecurityAdminRoleBucket = null;

        public ECSUtils(IConfiguration config,
                        IOptions<CloudFoundryApplicationOptions> appOptions,  // This is for illustration , it is not used in this example.
                        IOptions<CloudFoundryServicesOptions> servOptions, IHostingEnvironment env)
        {
            //readVariablesFromServiceOptions(servOptions);
            printConfiguration();

            if (env.IsDevelopment())
            {
                s3AccessKey = config.GetValue<String>("Access_key", null);
                s3SecretKey = config.GetValue<String>("Secret_Key", null);
                s3EndPoint = config.GetValue<String>("Endpoint", null);
                SecurityAdminRoleBucket = config.GetValue<String>("SecurityAdminRoleBucket", null);
            }
            else
            {
                s3AccessKey = config["Access_key"];
                s3SecretKey = config["Secret_Key"];
                s3EndPoint = config["Endpoint"];
                SecurityAdminRoleBucket = config["SecurityAdminRoleBucket"];
            }

        }

        private void readVariablesFromServiceOptions(IOptions<CloudFoundryServicesOptions> servOptions)
        {
            Dictionary<string, Service[]> serviceList = servOptions.Value.Services;
            if (serviceList != null && serviceList.LongCount() > 0)
            {
                Console.WriteLine(" Number of services bound to this application: " + serviceList.LongCount());
                // Lets get the type of service that we want.
                Service[] svList = serviceList.GetValueOrDefault("ecs-bucket");

                if (svList != null && svList.Length > 0)     // Cool, so atleast one service is bound..
                {
                    Console.WriteLine(" Number of ECS Bucket services bound to this application: " + svList.Length);
                    Service srvc = svList[0];
                    // Cool, so this is ECS service binding..
                    Dictionary<string, Credential> credentials = srvc.Credentials;
                    if (credentials != null)
                    {
                        Console.WriteLine(" Found ECS Bucket service : " + srvc.Name + ", " + srvc.Label + ", " + srvc.Plan);

                        Credential endPoint = credentials.GetValueOrDefault("endpoint");
                        Credential secretKey = credentials.GetValueOrDefault("secretKey");
                        Credential accessKey = credentials.GetValueOrDefault("accessKey");

                        s3AccessKey = accessKey.Value;
                        s3SecretKey = secretKey.Value;
                        s3EndPoint = endPoint.Value;
                    }
                }
            }
        }


        private AmazonS3Client GetAmazonS3Client()
        {
            AmazonS3Config amazonS3Config = new AmazonS3Config()
            {
                ServiceURL = s3EndPoint,
                ForcePathStyle = true
            };

            AmazonS3Client amazonS3Client = new AmazonS3Client(s3AccessKey, s3SecretKey, amazonS3Config);
            return amazonS3Client;
        }


        private AmazonS3Client GetS3Client()
        {
            return GetAmazonS3Client();
        }


        public async Task<PutObjectResponse> WriteSecurityAdminRoleToBucket(securityAdminRole g)
        {
            // Lets check to make sure the BUcket Exists.. otherwise create a new one.

            Task.Run(() => this.CheckCreateBucketAsync(SecurityAdminRoleBucket)).Wait();

            AmazonS3Client s3Client = GetS3Client();

            var por = new PutObjectRequest
            {
                BucketName = SecurityAdminRoleBucket,
                Key = g.SecurityAdminkey,
                ContentBody = JsonConvert.SerializeObject(g)
            };

            por.Metadata.Add("Type", "securityAdminRole");
            PutObjectResponse porr = await s3Client.PutObjectAsync(por);
            return porr;
            // System.Diagnostics.Debug.WriteLine("Object written: " + response1.ToString());
        }

        public async Task<List<securityAdminRole>> ReadSecurityAdminRoleFromBucket()
        {
            List<securityAdminRole> cList = new List<securityAdminRole>();
            AmazonS3Client s3Client = GetS3Client();

            ListObjectsRequest lor = new ListObjectsRequest()
            {
                BucketName = SecurityAdminRoleBucket,
            };

            ListObjectsResponse response1 = await s3Client.ListObjectsAsync(lor);
            List<S3Object> theObjects = response1.S3Objects;

            foreach (S3Object s3Obj in theObjects)
            {
                GetObjectRequest gor = new GetObjectRequest()
                {
                    BucketName = SecurityAdminRoleBucket,
                    Key = s3Obj.Key
                };

                GetObjectResponse gorr = await s3Client.GetObjectAsync(gor);
                StreamReader reader = new StreamReader(gorr.ResponseStream);
                string text = reader.ReadToEnd();
                securityAdminRole g = JsonConvert.DeserializeObject<securityAdminRole>(text);
                cList.Add(g);
            }

            return cList;
        }


        // Let us make sure that the bucket exists..
        private async Task CheckCreateBucketAsync(string bucketName)
        {
            AmazonS3Client s3Client = GetS3Client();

            try
            {
                GetBucketLocationResponse gblr = await s3Client.GetBucketLocationAsync(bucketName);

                if (gblr != null)
                {
                    Console.WriteLine("Bucket " + bucketName + " Exists @ " + gblr.Location.Value);
                }
            }
            catch (AmazonS3Exception se)
            {
                // Got an Exception so bucket may not exists..
                // Lets try creating the bucket..
                Console.WriteLine("Bucket " + bucketName + " Does not Exist, error code-> " + se.ErrorCode);

                PutBucketRequest pbr = new PutBucketRequest();
                pbr.BucketName = bucketName;
                PutBucketResponse pbrr = await s3Client.PutBucketAsync(pbr);
                Console.WriteLine("Bucket " + bucketName + " Created in client region.");
            }
        }

        private void printConfiguration()
        {
            Console.WriteLine("The values of the variables are as follows: S3EndPoint: " + s3EndPoint);
            Console.WriteLine("S3EndPoint  : " + s3EndPoint);
            Console.WriteLine("S3AccessKey : " + s3AccessKey);
            Console.WriteLine("S3SecretKey : " + s3SecretKey);
        }

    }
}
