using Azure;
using Azure.Data.Tables;

namespace JWT_Login_Authorization_DotNet.Models
{
    public class Candidate : ITableEntity
    {
        // the partition key can partition our data into groups and the
        // row key can identify an entity uniquely within a partition.
        public string Id { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }

        public string Seniority { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}