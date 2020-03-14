using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedKernel;

namespace Service.Mapping
{
    public class TestLogic : ITestLogic
    {
        private readonly IStorage _storage;

        public TestLogic(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<Test> CreateRootAsync(string name)
        {
            var storageRoot = await _storage.TestStorage.CreateRootAsync(name);
            var root = ToTest(storageRoot);
            return root;
        }

        public async Task<Test> CreateAsync(string name, string parentId)
        {
            if (string.IsNullOrWhiteSpace(parentId))
            {
                return await CreateRootAsync(name);
            }
            
            var storageTest = await _storage.TestStorage.CreateAsync(name, Guid.Parse(parentId));
            return ToTest(storageTest);
        }

        public async Task BuildTestTree(Test test)
        {
            await BuildTestTree(test, null);
        }

        private async Task BuildTestTree(Test test, Test parent)
        {
            var children = await _storage.TestStorage.GetChildren(Guid.Parse(test.Id));
            if (!children.Any()) return;

            test.Children = new List<Test>();
            foreach (var storageChild in children)
            {
                var child = ToTest(storageChild);
                test.Children.Add(child);
                await BuildTestTree(child, test);
            }
        }

        private static Test ToTest(StorageTest storageTest)
        {
            var test = new Test(storageTest.Id.ToString(), storageTest.Name);
            // TODO: More props
            return test;
        }
    }

    public interface ITestLogic
    {
        Task<Test> CreateRootAsync(string name);
        Task<Test> CreateAsync(string name, string parentId);
        Task BuildTestTree(Test test);
    }
}
