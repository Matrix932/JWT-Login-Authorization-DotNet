using Azure;
using Azure.Data.Tables;

namespace JWT_Login_Authorization_DotNet.Models
{
    public class Task : ITableEntity
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Level { get; set; }
        public string Title { get; set; }

        public string Code { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}