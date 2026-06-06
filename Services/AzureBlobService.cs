using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using TradePlatform.Api.DTOs;

namespace TradePlatform.Api.Services
{
    public class AzureBlobService
    {
        private readonly BlobServiceClient _blobService;
        private readonly string _containerName;

        public AzureBlobService(IConfiguration config)
        {
            _blobService = new BlobServiceClient(config["Azure:ConnectionString"]);
            _containerName = config["Azure:Container"];
        }
        // -----------------------------
        // 4. Delete blob from container
        // -----------------------------
        public async Task DeleteBlobAsync(FileDeleteRequestDto fdrDto)
        {
            var container = _blobService.GetBlobContainerClient(_containerName);
            var blob = container.GetBlobClient(fdrDto.file_name);
            await blob.DeleteIfExistsAsync(); // safe delete
        }

        // -----------------------------
        // 1. Generate SAS URL for UPLOAD
        // -----------------------------
        public string GetUploadSasUrl(string blobPath, string contentType)
        {
            var container = _blobService.GetBlobContainerClient(_containerName);
            var blob = container.GetBlobClient(blobPath);

            var sas = new BlobSasBuilder
            {
                BlobName = blobPath,
                Resource = "b",
                ExpiresOn = DateTime.UtcNow.AddMinutes(30),
                ContentType = contentType
            };

            // Upload requires Write + Create
            sas.SetPermissions(
                BlobSasPermissions.Write |
                BlobSasPermissions.Create
            );

            return blob.GenerateSasUri(sas).ToString();
        }
       
        // -----------------------------
        // 2. Generate SAS URL for VIEWING
        // -----------------------------
        public string GetReadSasUrl(string blobPath)
        {
            var container = _blobService.GetBlobContainerClient(_containerName);
            var blob = container.GetBlobClient(blobPath);

            var sas = new BlobSasBuilder
            {
                BlobName = blobPath,
                Resource = "b",
                ExpiresOn = DateTime.UtcNow.AddHours(12)
            };

            // Viewing requires Read
            sas.SetPermissions(BlobSasPermissions.Read);

            return blob.GenerateSasUri(sas).ToString();
        }

        // -----------------------------
        // 3. Clean blob URL (no SAS)
        // -----------------------------
        public string GetBlobUrl(string blobPath)
        {
            var baseUrl = _blobService.Uri.AbsoluteUri.TrimEnd('/');
            var container = _containerName.Trim('/');
            var path = blobPath.TrimStart('/');

            return $"{baseUrl}/{container}/{path}";
        }
    }
}
