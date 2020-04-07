using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace SharedKernel
{
    public class StorageTest : IOptimisticConcurrencyControlByETag, IUniquelyIdentifiable<Guid>, ITimeStamped
    {
        public Guid Id { get; set; }
        public DateTimeOffset RecordCreatedAt { get; set; }
        public DateTimeOffset RecordUpdatedAt { get; set; }
        public string Etag { get; set; }
        public Guid? ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public int State { get; set; }
        public string StateMessage { get; set; }
        public IDictionary<string, object> Properties { get; set; }
    }
}
