using Microsoft.AspNetCore.Mvc;
using SmartFlow.Connectors.API.Services;

namespace SmartFlow.Connectors.API
{
    public static class WebApplicationExtensions
    {

        internal static WebApplication MapApi(this WebApplication app)
        {
            var api = app.MapGroup("api");
            api.MapGet("snow/knowledge", ExecuteKnowledge);
            api.MapGet("snow/catalog", ExecuteCatelog);
            return app;
        }

        private static async Task<IResult> ExecuteKnowledge([FromServices] ServiceNowKnowledgeExtractor serviceNowKnowledgeExtractor, CancellationToken cancellationToken)
        {
            serviceNowKnowledgeExtractor.Execute();
            return Results.NoContent();
        }

        private static async Task<IResult> ExecuteCatelog([FromServices] ServiceNowKnowledgeExtractor serviceNowKnowledgeExtractor, CancellationToken cancellationToken)
        {
            serviceNowKnowledgeExtractor.ExecuteCatalogItems();
            return Results.NoContent();
        }
    }
}
