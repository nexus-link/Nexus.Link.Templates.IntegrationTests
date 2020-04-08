using System.Threading.Tasks;
using SharedKernel;

namespace Service.Models
{
    /// <summary>
    /// A class (Controller) that has some tests
    /// </summary>
    public interface ITestable
    {
        /// <summary>
        /// What test grouping it belongs to. See <see cref="TestGrouping"/>.
        /// </summary>
        string Group { get; }

        /// <summary>
        /// Run all tests and return a container test that wraps them.
        /// </summary>
        /// <param name="parent">The parent test (if any)</param>
        /// <returns>A test that wraps all other tests</returns>
        Task<Test> RunAllAsync(Test parent = null);
    }
}
