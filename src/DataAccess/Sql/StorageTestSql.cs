using System.Collections.Generic;
using Newtonsoft.Json;
using SharedKernel;

namespace DataAccess.Sql
{
    public class StorageTestSql : StorageTest
    {
        public string PropertiesJson { get; set; }

        public StorageTest ToStorageTest()
        {
            if (!string.IsNullOrWhiteSpace(PropertiesJson))
            {
                Properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(PropertiesJson);
            }

            return this;
        }

        public StorageTestSql() { }

        public StorageTestSql(StorageTest test)
        {
            Id = test.Id;
            RecordCreatedAt = test.RecordCreatedAt;
            RecordUpdatedAt = test.RecordUpdatedAt;
            Etag = test.Etag;
            ParentId = test.ParentId;
            Name = test.Name;
            Description = test.Description;
            FinishedAt = test.FinishedAt;
            State = test.State;
            StateMessage = test.StateMessage;
            PropertiesJson = test.Properties != null ? JsonConvert.SerializeObject(test.Properties) : null;
        }
    }
}
