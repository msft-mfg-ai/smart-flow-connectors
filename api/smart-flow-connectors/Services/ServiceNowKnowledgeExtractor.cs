using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;

namespace SmartFlow.Connectors.API.Services
{
    public class ServiceNowKnowledgeExtractor
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly IConfiguration _configuration; 
        public ServiceNowKnowledgeExtractor(BlobContainerClient blobContainerClient, IConfiguration configuration)
        {
            _blobContainerClient = blobContainerClient;
            _configuration = configuration;
        }

        public async Task Execute()
        {

            // 1. Get configuration values
            string serviceNowInstanceUrl = _configuration["ServiceNowInstanceUrl"];
            string serviceNowUsername = _configuration["ServiceNowUsername"];
            string serviceNowPassword = _configuration["ServiceNowPassword"];

            string serviceNowEndpoint = "/api/now/table/kb_knowledge?sysparm_limit=20";
            string blobName = $"kb_articles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

            // 2. Get knowledge articles from ServiceNow
            var articlesJson = await GetKnowledgeArticles(serviceNowInstanceUrl + serviceNowEndpoint, serviceNowUsername, serviceNowPassword);
            if (string.IsNullOrEmpty(articlesJson))
            {
                Console.WriteLine("No data returned from ServiceNow or an error occurred.");
                return;
            }

            // 3. Upload knowledge articles data to Azure Blob
            bool uploadSuccess = await UploadToAzureBlob(blobName, articlesJson);
            if (uploadSuccess)
            {
                Console.WriteLine($"Knowledge articles uploaded successfully to blob '{blobName}'");
            }
            else
            {
                Console.WriteLine("Failed to upload the articles to Azure Blob Storage.");
            }
        }

        private async Task<string> GetKnowledgeArticles(string apiUrl, string username, string password)
        {
            try
            {
                using var httpClient = new HttpClient();

                // Add basic auth header
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                // Make the GET request
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: Unable to retrieve data. HTTP Status: {response.StatusCode}");
                    return string.Empty;
                }

                // Read the response content as string (JSON)
                string content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred while retrieving articles from ServiceNow: " + ex.Message);
                return string.Empty;
            }
        }

        private async Task<bool> UploadToAzureBlob(string blobName, string jsonContent)
        {
            try
            {

                // Get a reference to the blob (file) we want to upload
                BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);

                // Convert the JSON content to a stream
                using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

                // Upload the blob
                await blobClient.UploadAsync(memoryStream, overwrite: true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred while uploading to Azure Blob: " + ex.Message);
                return false;
            }
        }
    }
}
