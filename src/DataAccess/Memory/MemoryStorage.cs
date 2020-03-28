using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using SharedKernel;

namespace DataAccess.Memory
{
    public class MemoryStorage : IStorage
    {
        public MemoryStorage(IMemoryCache cache, ILogger<MemoryStorage> logger)
        {
            TestStorage = new MemoryTestStorage(cache, logger);
        }

        public ITestStorage TestStorage { get; set; }
    }

    public class MemoryTestStorage : SlaveToMasterMemory<StorageTest, Guid>, ITestStorage
    {
        private readonly IMemoryCache _compensatingForParentIdsCannotBeNull;
        private readonly ILogger<MemoryStorage> _logger;
        private static readonly ConcurrentDictionary<Guid, StorageTest> AllTests = new ConcurrentDictionary<Guid, StorageTest>();

        public MemoryTestStorage(IMemoryCache cache, ILogger<MemoryStorage> logger)
        {
            _compensatingForParentIdsCannotBeNull = cache;
            _logger = logger;
        }

        public async Task<StorageTest> CreateRootAsync(string name)
        {
            var id = Guid.NewGuid();
            var fakeParentId = Guid.NewGuid();
            _compensatingForParentIdsCannotBeNull.Set(id, fakeParentId);

            var item = new StorageTest
            {
                Id = id,
                ParentId = null,
                Name = name,
                CreatedAt = DateTimeOffset.Now
            };
            var test = await CreateWithSpecifiedIdAndReturnAsync(fakeParentId, item.Id, item);
            AllTests.TryAdd(test.Id, test);
            return test;
        }

        public async Task<StorageTest> CreateAsync(string name, Guid parentId)
        {
            var item = new StorageTest
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                Name = name,
                CreatedAt = DateTimeOffset.Now
            };
            _compensatingForParentIdsCannotBeNull.Set(item.Id, parentId);

            var test = await CreateWithSpecifiedIdAndReturnAsync(parentId, item.Id, item);
            AllTests.TryAdd(test.Id, test);
            return test;
        }

        public async Task<List<StorageTest>> GetChildren(Guid id)
        {
            var children = await ReadChildrenAsync(id);
            return children.ToList();
        }

        public async Task<StorageTest> ReadAsync(Guid id)
        {
            if (!_compensatingForParentIdsCannotBeNull.TryGetValue(id, out Guid parentId))
            {
                return null;
            }

            var test = await ReadAsync(parentId, id);
            return test;
        }

        public async Task UpdateAsync(StorageTest storageTest)
        {
            if (_compensatingForParentIdsCannotBeNull.TryGetValue(storageTest.Id, out Guid parentId))
            {
                await UpdateAsync(parentId, storageTest.Id, storageTest);
            }
        }

        public Task<List<StorageTest>> GetOldTests(TimeSpan maxAge)
        {
            var old = new List<StorageTest>();
            foreach (var test in AllTests.Values)
            {
                if (test.CreatedAt.Add(maxAge) < DateTimeOffset.Now)
                {
                    old.Add(test);
                }
            }
            return Task.FromResult(old);
        }

        public async Task DeleteAsync(Guid id)
        {
            if (_compensatingForParentIdsCannotBeNull.TryGetValue(id, out Guid parentId))
            {
                await DeleteAsync(parentId, id);
                AllTests.TryRemove(id, out _);
            }
        }
    }
}
