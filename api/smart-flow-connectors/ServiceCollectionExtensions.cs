using Azure.Storage.Blobs;
using Azure;
using System.Diagnostics;
using System.Net.Sockets;
using Azure.Identity;
using SmartFlow.Connectors.API.Services;

namespace SmartFlow.Connectors.API
{

    internal static class ServiceCollectionExtensions
    {
        private static readonly DefaultAzureCredential _defaultAzureCredential = new DefaultAzureCredential();

        internal static IServiceCollection AddAzureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var storageAccountName = configuration["StorageAccountName"];
            if (!string.IsNullOrEmpty(storageAccountName))
            {
                var blobStorageEndpoint = $"https://{storageAccountName}.blob.core.windows.net";
                services.AddSingleton<BlobServiceClient>(sp =>
                {
                    var blobServiceClient = new BlobServiceClient(new Uri(blobStorageEndpoint), _defaultAzureCredential);
                    return blobServiceClient;
                });
            }
            services.AddTransient<ServiceNowKnowledgeExtractor>();
            return services;
        }
    }
}
