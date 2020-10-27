using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;
using SharedKernel;

namespace DataAccess.Sql
{
    public class SqlStorage : IStorage
    {
        public SqlStorage(string connectionString)
        {
            TestStorage = new SqlTestStorage(connectionString);
        }

        public ITestStorage TestStorage { get; set; }
    }

    public class SqlTestStorage : CrudSql<StorageTestSql, StorageTestSql>, ITestStorage
    {
        public SqlTestStorage(string connectionString) : base(connectionString, new SqlTableMetadata
        {
            TableName = "Test",
            CreatedAtColumnName = "RecordCreatedAt",
            UpdatedAtColumnName = "RecordUpdatedAt",
            EtagColumnName = "Etag",
            CustomColumnNames = new[] { "ParentId", "Name", "Description", "FinishedAt", "State", "StateMessage", "PropertiesJson" },
            OrderBy = new[] { "Name", "Id" }
        })
        {
        }

        public async Task<StorageTest> CreateRootAsync(string name)
        {
            return await CreateAndReturn(name, null);
        }

        public async Task<StorageTest> CreateAsync(string name, Guid parentId)
        {
            return await CreateAndReturn(name, parentId);
        }

        private async Task<StorageTest> CreateAndReturn(string name, Guid? parentId)
        {
            var item = new StorageTestSql
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                Name = name,
                RecordCreatedAt = DateTimeOffset.Now,
                State = (int) StateEnum.Waiting
            };

            var test = await base.CreateAndReturnAsync(item);
            return test.ToStorageTest();
        }

        public async Task<List<StorageTest>> GetChildren(Guid id)
        {
            var children = await base.SearchWhereAsync("ParentId = @id", "RecordCreatedAt ASC", new { id });
            return children.Data.Select(x => x.ToStorageTest()).ToList();
        }

        public async Task<StorageTest> ReadAsync(Guid id)
        {
            var test = await base.ReadAsync(id);
            return test?.ToStorageTest();
        }

        public async Task UpdateAsync(StorageTest storageTest)
        {
            await base.UpdateAsync(storageTest.Id, new StorageTestSql(storageTest));
        }

        public async Task<List<StorageTest>> GetOldTests(TimeSpan maxAge)
        {
            var allTests = await base.ReadAllAsync();
            return allTests
                .Where(test => test.RecordCreatedAt.Add(maxAge) < DateTimeOffset.Now)
                .Select(x => x.ToStorageTest())
                .ToList();
        }

        public async Task DeleteAsync(Guid id)
        {
            await base.ExecuteAsync("UPDATE Test SET ParentId = NULL WHERE ParentId = @id", new { id });
            await base.DeleteAsync(id);
        }
    }
}
