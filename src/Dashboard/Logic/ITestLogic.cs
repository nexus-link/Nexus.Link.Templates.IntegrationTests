using System.Text.Json;
using System.Threading.Tasks;

namespace Dashboard.Logic
{
    public interface ITestLogic
    {
        Task<JsonElement> GetTestAsync(string testId);
        Task<JsonElement> CreateTest();
    }
}
