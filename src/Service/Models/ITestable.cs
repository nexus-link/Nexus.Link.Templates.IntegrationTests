using System.Threading.Tasks;
using SharedKernel;

namespace Service.Models
{
    public interface ITestable
    {
        string Group { get; }
        Task<Test> RunAllAsync(string parentId = null);
    }
}
