using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedKernel
{
    public interface IStorage
    {
        ITestStorage TestStorage { get; set; }
    }

    public interface ITestStorage
    {
        Task<StorageTest> CreateRootAsync(string name);
        Task<StorageTest> CreateAsync(string name, Guid parentId);
        Task<List<StorageTest>> GetChildren(Guid id);
    }
}
