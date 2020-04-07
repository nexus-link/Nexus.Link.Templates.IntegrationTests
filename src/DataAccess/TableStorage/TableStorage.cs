using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Azure.Storage.Table;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Storage.Logic;
using SharedKernel;

namespace DataAccess.TableStorage
{
    public class TableStorage : IStorage
    {
        public TableStorage(string connectionString)
        {
            TestStorage = new TableTestStorage(connectionString, "PlatformIntegrationTestsStorageV1");
        }

        public ITestStorage TestStorage { get; set; }
    }

    public class TableTestStorage : AzureStorageTable<StorageTest, StorageTest>, ITestStorage
    {
        private const string PartitionKey = "tests";

        private static string ParentKey(Guid parentId)
        {
            return $"parent-{parentId}";
        }

        public TableTestStorage(string connectionString, string name) : base(connectionString, name)
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
            var item = new StorageTest
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                Name = name,
                RecordCreatedAt = DateTimeOffset.Now
            };

            await CreateAsync(PartitionKey, item.Id.ToString(), item);
            if (item.ParentId != null)
            {
                var parent = await ReadAsync(PartitionKey, item.ParentId.ToString());
                await CreateAsync(ParentKey(parent.Id), item.Id.ToString(), item);
            }

            var test = await ReadAsync(PartitionKey, item.Id.ToString());
            return test;
        }

        public async Task<List<StorageTest>> GetChildren(Guid id)
        {
            var children = await ReadAllWithPagingAsync(ParentKey(id));
            return children.Data.ToList();
        }

        public async Task<StorageTest> ReadAsync(Guid id)
        {
            var test = await ReadAsync(PartitionKey, id.ToString());
            return test;
        }

        public async Task UpdateAsync(StorageTest item)
        {
            await UpdateAsync(PartitionKey, item.Id.ToString(), item);
        }

        public async Task<List<StorageTest>> GetOldTests(TimeSpan maxAge)
        {
            var old = new List<StorageTest>();

            const int limit = 100;
            var enumerator = new PageEnvelopeEnumeratorAsync<StorageTest>((offset, ct) => ReadAllWithPagingAsync(PartitionKey, offset, limit, ct));
            while (await enumerator.MoveNextAsync())
            {
                var item = enumerator.Current;
                if (item.RecordCreatedAt.Add(maxAge) < DateTimeOffset.Now)
                {
                    old.Add(item);
                }
            }
            return old;
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var test = await ReadAsync(id);
                if (test == null)
                {
                    Log.LogWarning($"When deleting test {id}, it was not found");
                    return;
                }

                await DeleteAsync(PartitionKey, id.ToString());

                // TODO: Delete from ParentKey
            }
            catch (Exception e)
            {
                Log.LogError($"Unable to DELETE test {id}: {e.Message}", e);
            }
        }
    }
}
