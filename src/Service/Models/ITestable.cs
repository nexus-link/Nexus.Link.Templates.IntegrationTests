using System.Threading.Tasks;
using SharedKernel;

namespace Service.Models
{
    public interface ITestable
    {
        string Group { get; }
        Task<Test> RunAllAsync(Test parent = null);
    }
}
