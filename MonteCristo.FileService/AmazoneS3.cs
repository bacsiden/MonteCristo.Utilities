using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MonteCristo.FileService
{
    public class AmazoneS3 : IFileService
    {
        private IConfiguration _configuration { get; }
        private readonly string bucketName;
        private readonly string awsBucketRegion = "https://s3-ap-southeast-1.amazonaws.com";
        private AmazonS3Client CreateAmazonS3Client()
        {
            var awsAccessKeyId = _configuration.GetSection("AppSettings:awsAccessKeyId").Value;
            var awsSecretAccessKey = _configuration.GetSection("AppSettings:awsSecretAccessKey").Value;

            return new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.APSoutheast1);
        }

        public AmazoneS3(IConfiguration configuration)
        {
            _configuration = configuration;
            bucketName = _configuration.GetSection("AppSettings:awsBucket").Value;
        }

        public string GetFullPath(string fileName)
        {
            var tail = awsBucketRegion.EndsWith("/") ? null : "/";
            return $"{awsBucketRegion}{tail}{bucketName}/{fileName}";
        }



        public async Task<string> UpsertAsync(Stream inputStream, string fileName, string rootFolder, bool createSubDateFolder = true)
        {
            var subFolder = createSubDateFolder ? $"{DateTime.Now.ToString("yyyy/MM")}/" : null;
            rootFolder = string.IsNullOrEmpty(rootFolder) ? null : $"{rootFolder}/";
            fileName = $"{rootFolder}{subFolder}{fileName}";

            using (var client = CreateAmazonS3Client())
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    InputStream = inputStream
                };
                await client.PutObjectAsync(putRequest);
            }

            return fileName;
        }

        public Task<string> UpsertAsync(string base64String, string fileName, string rootFolder, bool createSubDateFolder = true)
        {
            byte[] data = Convert.FromBase64String(base64String);

            MemoryStream memoryStream = new MemoryStream(data);
            return UpsertAsync(memoryStream, fileName, rootFolder, createSubDateFolder);
        }

        public Task DeleteAsync(string path)
        {
            if (string.IsNullOrEmpty(path)) return Task.CompletedTask;

            using (var client = CreateAmazonS3Client())
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = path
                };
                return client.DeleteObjectAsync(deleteRequest);
            }
        }
    }
}
