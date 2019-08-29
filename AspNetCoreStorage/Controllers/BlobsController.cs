using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection;

namespace AspNetCoreStorage.Controllers
{
    public class BlobsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private CloudBlobContainer GetCloudBlobContainer()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot Configuration = builder.Build();
            
            //CloudStorageAccount storageAccount =
            //    CloudStorageAccount.Parse(Configuration["ConnectionStrings:aspnettutorial_AzureStorageConnectionString"]);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:netcorestorageaccountstring"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("aspnetcoreblob");
            return container;
        }


        public ActionResult CreateBlobContainer()
        {
            CloudBlobContainer container = GetCloudBlobContainer();
            ViewBag.Success = container.CreateIfNotExistsAsync().Result;
            ViewBag.BlobContainerName = container.Name;
            
            return View();
        }

        public string UploadBlob()
        {
            //string dir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string dir = System.Environment.CurrentDirectory;
            string fileName = "luffy.jpg";
            var path = Path.Combine(dir, fileName);

            //var path = @"D:\Azure Projects\Azure POC 1\AspNetCoreStorage\Sample.txt";
            //var path = @"~\AspNetCoreStorage\Sample.txt";

            //var name = Path.GetFileName(path);

            CloudBlobContainer container = GetCloudBlobContainer();
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

            using (var fileStream = System.IO.File.OpenRead(path))
            {
                blob.UploadFromStreamAsync(fileStream).Wait();
            }
            return "success!";
        }

        public ActionResult ListBlobs()
        {
            CloudBlobContainer container = GetCloudBlobContainer();
            List<string> blobs = new List<string>();
            BlobResultSegment resultSegment = container.ListBlobsSegmentedAsync(null).Result;
            foreach (IListBlobItem item in resultSegment.Results)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    blobs.Add(blob.Name);
                }
                else if (item.GetType() == typeof(CloudPageBlob))
                {
                    CloudPageBlob blob = (CloudPageBlob)item;
                    blobs.Add(blob.Name);
                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory dir = (CloudBlobDirectory)item;
                    blobs.Add(dir.Uri.ToString());
                }
            }

            return View(blobs);
        }
    }
}