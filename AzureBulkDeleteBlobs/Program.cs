using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SocialFruits.Extensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeleteBlobs
{
    class Program
    {
        private static void showHelpInto()
        {
            Console.WriteLine("arguments missing: <file name or extension pattern> <folder path> <container 1> <container 2> .. <container n> -verbose");
            Console.WriteLine("<search pattern> examples:");
            Console.WriteLine(" - \".log\" Looks for files with extensino .log ");
            Console.WriteLine(" - \"setup.log\" Looks for files with this exact name");
            Console.WriteLine("< folder path >: Examples: ");
            Console.WriteLine(" - \"public\" Looks for files in this path ");
            Console.WriteLine(" - \"public\\logs\" Looks for files in this path ");
            Console.WriteLine("<container 1> <container 2> ... etc: list of blob container name(s) to look in");
            Console.WriteLine("-verbose: list URL of any blob that is found");
        }

        static void Main(string[] args)
        {
            if(!args.Any())
            {
                showHelpInto();
                return;
            }

            bool isVerbose = args.Any(a => a.ToLower() == "-verbose");

            string fileSearchPattern = args[0];

            string pathSearchPattern = args[1];

            //Get container names
            //Simply remove all paramenters we've collected. The remaining will be container names
            var containerNames = args.Where(a => a != fileSearchPattern && a != pathSearchPattern && a != "-verbose");

            if(!containerNames.Any())
            {
                showHelpInto();
                return;
            }


            Console.WriteLine($"Searching for blobs with file pattern: \"{fileSearchPattern}\" inside path \"{pathSearchPattern}\" ");

            CloudStorageAccount account = CloudStorageAccount.Parse(ConfigurationHelper.ConfigurationValue("AzureStorageConnectionString"));
            CloudBlobClient blobClient = account.CreateCloudBlobClient();


            foreach (var containerName in containerNames)
            {
                Console.WriteLine($"Searching Blob Container: {containerName.ToLower()}");
                var container = blobClient.GetContainerReference(containerName.ToLower());

                var perm = new BlobContainerPermissions();
                perm.PublicAccess = BlobContainerPublicAccessType.Blob;

                container.SetPermissions(perm);

                var kmlBlobs = container.ListBlobs(useFlatBlobListing: true, blobListingDetails: BlobListingDetails.None, prefix: pathSearchPattern)
                    .OfType<CloudBlockBlob>()
                    .Where(b => b.Name.ToLower().EndsWith(fileSearchPattern));

                int deletedCount = 0;
#if !DEBUG
                Parallel.ForEach(kmlBlobs, blob =>
#else
                foreach (var blob in kmlBlobs)
#endif
                {
                    if (deletedCount>0 && deletedCount % 5000 == 0)
                        Console.WriteLine($"Deleted {deletedCount} blobs so far");
                    if (isVerbose)
                        Console.WriteLine($"Found file {blob.Uri.ToString()}");
                    if (blob.Uri.PathAndQuery.Contains(fileSearchPattern))
                    {
                        if (isVerbose)
                            Console.WriteLine("    Deleting");
                        if (blob.DeleteIfExists())
                            deletedCount++;
                        else
                        {
                            Console.WriteLine($"Deleting blob failed {blob.Uri.ToString()}");
                        }
                    }
                }
#if !DEBUG
                
                );
#endif
            }
        }
    }
}
