using System.Threading.Tasks;
using Dashboard.Logic;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Hubs
{
    public class TestsHub : Hub
    {
        private readonly ITestLogic _testLogic;

        public TestsHub(ITestLogic testLogic)
        {
            _testLogic = testLogic;
        }

        public async Task UpdateStatus(string testId)
        {
            var test = await _testLogic.GetTestAsync(testId);
            await Clients.All.SendAsync("TestChanged", test);
        }
    }
}
