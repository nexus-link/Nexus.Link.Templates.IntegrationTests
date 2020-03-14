using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using SharedKernel;

namespace DataAccess.Memory
{
    public class MemoryStorage : IStorage
    {
        public MemoryStorage()
        {
            TestStorage = new MemoryTestStorage();
        }

        public ITestStorage TestStorage { get; set; }
    }

    public class MemoryTestStorage : SlaveToMasterMemory<StorageTest, Guid>, ITestStorage
    {
        public async Task<StorageTest> CreateRootAsync(string name)
        {
            var item = new StorageTest
            {
                Id = Guid.NewGuid(),
                ParentId = null,
                Name = name
            };
            var test = await CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), item.Id, item);
            return test;
        }

        public async Task<StorageTest> CreateAsync(string name, Guid parentId)
        {
            var item = new StorageTest
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                Name = name
            };
            var test = await CreateWithSpecifiedIdAndReturnAsync(parentId, item.Id, item);
            return test;
        }

        public async Task<List<StorageTest>> GetChildren(Guid id)
        {
            var children = await ReadChildrenAsync(id);
            return children.ToList();
        }
    }
}
