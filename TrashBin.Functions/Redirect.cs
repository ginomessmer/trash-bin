using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TrashBin.Entities;

namespace TrashBin.Functions
{
    public static class Redirect
    {
        [FunctionName(nameof(Redirect))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "go/{id}")] HttpRequest req,
            [Table(TrashDefaults.UploadsTableName, TrashDefaults.UrlPartitionKey, "{id}")] UrlEntity entity) => 
                entity is null ? (IActionResult) new NotFoundResult() : new RedirectResult(entity.Url);
    }
}