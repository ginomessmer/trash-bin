using System;
using Microsoft.Azure.Cosmos.Table;

namespace TrashBin.Functions
{
    public class UrlEntity : TableEntity
    {
        public string Url { get; set; }

        public UrlEntity()
        {
        }
        
        public UrlEntity(string url)
        {
            PartitionKey = TrashDefaults.UrlPartitionKey;
            Url = url;
            RowKey = Guid.NewGuid().ToString();
            Timestamp = DateTimeOffset.UtcNow;
        }
    }
}