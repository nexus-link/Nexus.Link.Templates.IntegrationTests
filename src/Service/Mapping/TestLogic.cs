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
            var root = await ToTestRecursive(storageRoot);
            return root;
        }

        public async Task<Test> CreateAsync(string name, Test parent)
        {
            if (parent == null)
            {
                return await CreateRootAsync(name);
            }
            
            var storageTest = await _storage.TestStorage.CreateAsync(name, Guid.Parse(parent.Id));

            var test = await ToTestRecursive(storageTest);
            if (parent.Children == null) parent.Children = new List<Test>();
            parent.Children.Add(test);

            return test;
        }

        public async Task SetState(Test test, StateEnum state, string message)
        {
            test.InternalState = state;
            test.StateMessage = message;

            var storageTest = await _storage.TestStorage.ReadAsync(Guid.Parse(test.Id));
            storageTest.State = (int)test.InternalState;
            storageTest.StateMessage = test.StateMessage;
            await _storage.TestStorage.UpdateAsync(storageTest);
        }

        public async Task<Test> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var storageTest = await _storage.TestStorage.ReadAsync(Guid.Parse(id));
            if (storageTest == null) return null;
            var test = await ToTestRecursive(storageTest);
            return test;
        }

        public async Task<int> PurgeAsync(TimeSpan maxAge)
        {
            var testsToDelete = await _storage.TestStorage.GetOldTests(maxAge);
            foreach (var test in testsToDelete)
            {
                await _storage.TestStorage.DeleteAsync(test.Id);
            }
            return testsToDelete.Count;
        }

        private async Task<Test> ToTestRecursive(StorageTest storageTest)
        {
            var test = new Test(storageTest.Id.ToString(), storageTest.Name, storageTest.Description)
            {
                CreatedAt = storageTest.CreatedAt,
                FinishedAt = storageTest.FinishedAt,
                InternalState = (StateEnum) storageTest.State,
                StateMessage = storageTest.StateMessage,
                Properties = storageTest.Properties
            };
            await AddChildrenRecursiveAsync(test);
            return test;
        }

        private async Task AddChildrenRecursiveAsync(Test test)
        {
            var children = await _storage.TestStorage.GetChildren(Guid.Parse(test.Id));
            if (!children.Any()) return;

            test.Children = new List<Test>();
            foreach (var storageChild in children)
            {
                var child = await ToTestRecursive(storageChild);
                test.Children.Add(child);
            }
        }

    }

    public interface ITestLogic
    {
        Task<Test> CreateRootAsync(string name);
        Task<Test> CreateAsync(string name, Test parent);
        Task SetState(Test test, StateEnum state, string message);
        Task<Test> Get(string id);
        Task<int> PurgeAsync(TimeSpan maxAge);
    }
}
