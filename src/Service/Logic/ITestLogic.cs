using System;
using System.Threading.Tasks;
using SharedKernel;

namespace Service.Logic
{
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