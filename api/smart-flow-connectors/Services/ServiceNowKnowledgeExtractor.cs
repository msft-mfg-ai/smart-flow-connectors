using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using static System.Net.WebRequestMethods;

namespace SmartFlow.Connectors.API.Services
{
    public class ServiceNowKnowledgeExtractor
    {
        private readonly BlobContainerClient _knowledgeBlobContainerClient;
        private readonly BlobContainerClient _catalogBlobContainerClient;
        private readonly IConfiguration _configuration; 
        public ServiceNowKnowledgeExtractor(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _configuration = configuration;

            _knowledgeBlobContainerClient = blobServiceClient.GetBlobContainerClient(_configuration["KnowledgeContentStorageContainer"]);
            _catalogBlobContainerClient = blobServiceClient.GetBlobContainerClient(_configuration["CatalogContentStorageContainer"]);
        }

        public async Task Execute()
        {

            // 1. Get configuration values
            string serviceNowInstanceUrl = _configuration["ServiceNowInstanceUrl"];
            string serviceNowUsername = _configuration["ServiceNowUsername"];
            string serviceNowPassword = _configuration["ServiceNowPassword"];

            //string serviceNowEndpoint = "/api/now/table/kb_knowledge?sysparm_limit=20";
            string serviceNowEndpoint = "/api/now/table/kb_knowledge?sysparm_query=active%3Dtrue%5Eworkflow_state%3Dpublished&sysparm_fields=number%2Ctext%2Ctopic%2Ccategory%2Cshort_description&sysparm_limit=100";
            string blobName = $"kb_articles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

            // 2. Get knowledge articles from ServiceNow
            var articlesJson = await GetServiceNowData(serviceNowInstanceUrl + serviceNowEndpoint, serviceNowUsername, serviceNowPassword);
            if (string.IsNullOrEmpty(articlesJson))
            {
                Console.WriteLine("No data returned from ServiceNow or an error occurred.");
                return;
            }

            // 3. Upload knowledge articles data to Azure Blob
            bool uploadSuccess = await UploadToAzureBlob(_knowledgeBlobContainerClient, blobName, articlesJson);
            if (uploadSuccess)
            {
                Console.WriteLine($"Knowledge articles uploaded successfully to blob '{blobName}'");
            }
            else
            {
                Console.WriteLine("Failed to upload the articles to Azure Blob Storage.");
            }
        }

        public async Task ExecuteCatalogItems()
        {

            // 1. Get configuration values
            string serviceNowInstanceUrl = _configuration["ServiceNowInstanceUrl"];
            string serviceNowUsername = _configuration["ServiceNowUsername"];
            string serviceNowPassword = _configuration["ServiceNowPassword"];

            //string serviceNowEndpoint = "/api/now/table/kb_knowledge?sysparm_limit=20";
            string serviceNowEndpoint = "/api/now/table/sc_cat_item?sysparm_query=type!%3Dbundle%5Esys_class_name!%3Dsc_cat_item_guide%5Etype!%3Dpackage%5Esys_class_name!%3Dsc_cat_item_content%5Eactive%3Dtrue&sysparm_fields=sys_name%2Cshort_description%2Cdescription%2Csys_id%2Csys_tags%2Ccategory%2Cname%2Crequest_method%2Ctaxonomy_topic%2Clocation&sysparm_limit=250";
            string blobName = $"catalog_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

            // 2. Get knowledge articles from ServiceNow
            var articlesJson = await GetServiceNowData(serviceNowInstanceUrl + serviceNowEndpoint, serviceNowUsername, serviceNowPassword);
            if (string.IsNullOrEmpty(articlesJson))
            {
                Console.WriteLine("No data returned from ServiceNow or an error occurred.");
                return;
            }

            // 3. Upload knowledge articles data to Azure Blob
            bool uploadSuccess = await UploadToAzureBlob(_catalogBlobContainerClient, blobName, articlesJson);
            if (uploadSuccess)
            {
                Console.WriteLine($"Knowledge articles uploaded successfully to blob '{blobName}'");
            }
            else
            {
                Console.WriteLine("Failed to upload the articles to Azure Blob Storage.");
            }
        }

        private async Task<string> GetServiceNowData(string apiUrl, string username, string password)
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

        private async Task<bool> UploadToAzureBlob(BlobContainerClient blobContainerClient, string blobName, string jsonContent)
        {
            try
            {

                // Get a reference to the blob (file) we want to upload
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

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
