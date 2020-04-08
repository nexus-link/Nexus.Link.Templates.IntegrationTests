using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedKernel;

namespace Service.Mapping
{
    /// <summary>
    /// Maps models from outside world to storage world
    /// </summary>
    public class TestLogic : ITestLogic
    {
        private readonly IStorage _storage;

        /// <summary></summary>
        public TestLogic(IStorage storage)
        {
            _storage = storage;
        }

        /// <inheritdoc />
        public async Task<Test> CreateRootAsync(string name)
        {
            var storageRoot = await _storage.TestStorage.CreateRootAsync(name);
            var root = await ToTestRecursive(storageRoot);
            return root;
        }

        /// <inheritdoc />
        public async Task<Test> CreateAsync(string name, Test parent)
        {
            if (!Guid.TryParse(parent?.Id, out _))
            {
                return await CreateRootAsync(name);
            }

            // ReSharper disable once PossibleNullReferenceException
            var storageTest = await _storage.TestStorage.CreateAsync(name, Guid.Parse(parent.Id));

            var test = await ToTestRecursive(storageTest);
            if (parent.Children == null) parent.Children = new List<Test>();
            parent.Children.Add(test);

            return test;
        }

        /// <inheritdoc />
        public async Task SetStateAsync(Test test, StateEnum state, string message)
        {
            test.InternalState = state;
            test.StateMessage = message;
            await UpdateAsync(test);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Test test)
        {
            var storageTest = await _storage.TestStorage.ReadAsync(Guid.Parse(test.Id));

            storageTest.Name = test.Name;
            storageTest.Description = test.Description;
            storageTest.FinishedAt = test.FinishedAt;
            storageTest.RecordUpdatedAt = DateTimeOffset.Now;
            storageTest.State = (int)test.InternalState;
            storageTest.StateMessage = test.StateMessage;
            storageTest.Properties = test.Properties;

            await _storage.TestStorage.UpdateAsync(storageTest);
        }

        /// <inheritdoc />
        public async Task<Test> GetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var storageTest = await _storage.TestStorage.ReadAsync(Guid.Parse(id));
            if (storageTest == null) return null;
            var test = await ToTestRecursive(storageTest);
            return test;
        }

        /// <inheritdoc />
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
                CreatedAt = storageTest.RecordCreatedAt,
                FinishedAt = storageTest.FinishedAt,
                InternalState = (StateEnum)storageTest.State,
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

    /// <summary></summary>
    public interface ITestLogic
    {
        /// <summary>
        /// Create a test without parent
        /// </summary>
        Task<Test> CreateRootAsync(string name);

        /// <summary>
        /// Create a test (with a parent)
        /// </summary>
        Task<Test> CreateAsync(string name, Test parent);

        /// <summary>
        /// Set the state of a test
        /// </summary>
        Task SetStateAsync(Test test, StateEnum state, string message);

        /// <summary>
        /// Save a state to storage
        /// </summary>
        Task UpdateAsync(Test test);

        /// <summary>
        /// Get a test by id
        /// </summary>
        Task<Test> GetAsync(string id);

        /// <summary>
        /// Remove all tests older than a time span
        /// </summary>
        Task<int> PurgeAsync(TimeSpan maxAge);
    }
}
