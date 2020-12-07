using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TrashBin.Functions
{
    public static class Shorten
    {
        [FunctionName("Shorten")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload")] HttpRequest req,
            [Table(TrashDefaults.UploadsTableName)] CloudTable table, Binder binder,
            ILogger log)
        {
            await table.CreateIfNotExistsAsync();
            
            var form = await req.ReadFormAsync();

            if (!form.TryGetValue("input", out var inputValue))
                return new BadRequestResult();

            if (!Uri.TryCreate(inputValue, UriKind.RelativeOrAbsolute, out var uri))
                return new BadRequestResult();

            
            var entity = new UrlEntity(uri.ToString());

            var output = await binder.BindAsync<IAsyncCollector<UrlEntity>>(new Attribute[]
            {
                new StorageAccountAttribute("AzureWebJobsStorage"),
                new TableAttribute(TrashDefaults.UploadsTableName, entity.PartitionKey, entity.RowKey)
            });

            try
            {
                await output.AddAsync(entity);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error while inserting into table");
                throw;
            }

            return new CreatedResult($"{req.Scheme}://{req.Host}/api/go/{entity.RowKey}", entity);
        }
    }
}
