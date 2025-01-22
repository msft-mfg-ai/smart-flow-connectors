using Microsoft.AspNetCore.Mvc;
using SmartFlow.Connectors.API.Services;

namespace SmartFlow.Connectors.API
{
    public static class WebApplicationExtensions
    {

        internal static WebApplication MapApi(this WebApplication app)
        {
            var api = app.MapGroup("api");
            api.MapGet("snow/get", ExecuteSnow);
            return app;
        }

        private static async Task<IResult> ExecuteSnow([FromServices] ServiceNowKnowledgeExtractor serviceNowKnowledgeExtractor, CancellationToken cancellationToken)
        {
            serviceNowKnowledgeExtractor.Execute();
            return Results.NoContent();
        }
    }
}
