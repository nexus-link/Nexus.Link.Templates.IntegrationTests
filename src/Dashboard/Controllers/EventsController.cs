using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private static readonly ConcurrentDictionary<string, Stat> Stats = new ConcurrentDictionary<string, Stat>();

        [HttpPost("{entityName}/{eventName}/{major}")]
        public async Task Subscribe(string entityName, string eventName, int major)
        {
            var key = $"{entityName}/{eventName}";
            if (!Stats.TryGetValue(key, out _)) Stats.TryAdd(key, new Stat());
            Stats[key].Count++;

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
            var ordered = GetOrderedStats();
            var counts = ordered.Values.Select(x => x.Count);
            await _hubContext.Clients.All.SendAsync("NewEventsStats", ordered.Keys, counts);
        }

        public static Dictionary<string, Stat> GetOrderedStats()
        {
            return Stats.OrderBy(x => x.Value.FirstEncounter).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public class Stat
    {
        public int Count { get; set; }
        public DateTimeOffset FirstEncounter { get; set; } = DateTimeOffset.Now;
    }
}