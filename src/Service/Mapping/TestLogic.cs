using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

        public async Task BuildTestTreeAsync(Test test)
        {
            var children = await _storage.TestStorage.GetChildren(Guid.Parse(test.Id));
            if (!children.Any()) return;

            test.Children = new List<Test>();
            foreach (var storageChild in children)
            {
                var child = ToTest(storageChild);
                test.Children.Add(child);
                await BuildTestTreeAsync(child);
            }
        }

        public async Task SetState(Test test, StateEnum state, string message)
        {
            var storageTest = await _storage.TestStorage.ReadAsync(Guid.Parse(test.Id));
            storageTest.State = (int) state;
            storageTest.StateMessage = message;
            await _storage.TestStorage.UpdateAsync(storageTest);
            test.State = state;
            test.StateMessage = message;
        }

        private static Test ToTest(StorageTest storageTest)
        {
            var test = new Test(storageTest.Id.ToString(), storageTest.Name, storageTest.Description)
            {
                CreatedAt = storageTest.CreatedAt,
                FinishedAt = storageTest.FinishedAt,
                State = (StateEnum) storageTest.State,
                StateMessage = storageTest.StateMessage,
                Properties = storageTest.Properties
            };
            return test;
        }
    }

    public interface ITestLogic
    {
        Task<Test> CreateRootAsync(string name);
        Task<Test> CreateAsync(string name, string parentId);
        Task BuildTestTreeAsync(Test test);
        Task SetState(Test test, StateEnum state, string message);
    }
}
