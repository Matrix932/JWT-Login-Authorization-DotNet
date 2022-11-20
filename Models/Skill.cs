using Azure;
using Azure.Data.Tables;

namespace JWT_Login_Authorization_DotNet.Models
{
    public class Skill : ITableEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Tasks { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}