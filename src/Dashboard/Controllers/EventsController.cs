using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Dashboard.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IHubContext<EventHub> _hubContext;

        public EventsController(IHubContext<EventHub> hubContext)
        {
            _hubContext = hubContext;
        }

        private static readonly ConcurrentDictionary<string, int> Stats = new ConcurrentDictionary<string, int>();

        [HttpPost("{entityName}/{eventName}/{major}")]
        public async Task Subscribe(string entityName, string eventName, int major)
        {
            var key = $"{entityName}/{eventName}";
            if (!Stats.TryGetValue(key, out _)) Stats.TryAdd(key, 0);
            Stats[key]++;

            await SendSignal();
        }

        [HttpPost("Reset")]
        public async Task ResetStats()
        {
            Stats.Clear();

            await SendSignal();
        }

        private async Task SendSignal()
        {
            var ordered = Stats.OrderBy(x => x.Key.ToLowerInvariant()).ToDictionary(x => x.Key, x => x.Value);
            await _hubContext.Clients.All.SendAsync("NewEventsStats", ordered.Keys, ordered.Values);
        }
    }
}