using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileTypeChecker;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Primitives;
using MimeKit;
using TrashBin.Entities;

namespace TrashBin.Functions
{
    public class Upload
    {
        private Binder _binder;
        private ILogger _logger;
        private HttpRequest _request;


        [FunctionName(nameof(Upload))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload")] HttpRequest req,
            [Table(TrashDefaults.UploadsTableName)] CloudTable table, Binder binder,
            [Blob("drop/{rand-guid}", FileAccess.Write)] ICloudBlob blob,
            ILogger log)
        {
            _binder = binder;
            _logger = log;
            _request = req;
            
            await table.CreateIfNotExistsAsync();
            var form = await req.ReadFormAsync();

            if (form.TryGetValue("input", out var inputValue))
            {
                if (Uri.TryCreate(inputValue, UriKind.Absolute, out var uri))
                    return await UploadUrlAsync(uri);

                return await UploadTextAsync(inputValue, blob);
            }

            var file = form.Files.SingleOrDefault(f => f.Name == "blob");
            if (file != null)
                return await UploadFileAsync(file, blob);

            return new BadRequestResult();
        }

        private async Task<IActionResult> UploadTextAsync(string text, ICloudBlob blob)
        {
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(text);
            await writer.FlushAsync();

            stream.Position = 0;

            blob.Properties.ContentType = "text";
            await blob.UploadFromStreamAsync(stream);
            return new CreatedResult(blob.Uri, new { blob.Uri, blob.Properties.ContentType });
        }

        private async Task<IActionResult> UploadFileAsync(IFormFile file, ICloudBlob blob)
        {
            await using var stream = file.OpenReadStream();
            var contentType = MimeTypes.GetMimeType(file.FileName);

            blob.Properties.ContentType = contentType;
            await blob.UploadFromStreamAsync(stream);
            return new CreatedResult(blob.Uri, new { blob.Uri, blob.Properties.ContentType });
        }

        private async Task<IActionResult> UploadUrlAsync(Uri uri)
        {
            var entity = new UrlEntity(uri.ToString());

            var output = await _binder.BindAsync<IAsyncCollector<UrlEntity>>(new Attribute[]
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
                _logger.LogError(ex, "Error while inserting into table");
                throw;
            }

            return new CreatedResult($"{_request.Scheme}://{_request.Host}/api/go/{entity.RowKey}", entity);
        }
    }
}
