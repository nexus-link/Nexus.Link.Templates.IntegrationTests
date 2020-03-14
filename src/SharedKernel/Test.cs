using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharedKernel
{
    public class Test
    {
        [JsonProperty(Order = 1)]
        public string Id { get; set; }

        [JsonProperty(Order = 2)]
        public string Name { get; set; }

        [JsonProperty(Order = 3)]
        public string Description { get; set; }

        [JsonProperty(Order = 4)]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty(Order = 5)]
        public DateTimeOffset? FinishedAt { get; set; }

        [JsonProperty(Order = 6)]
        public StateEnum State { get; set; }

        [JsonProperty(Order = 7)]
        public string StateMessage { get; set; }

        [JsonProperty(Order = 8)]
        public IDictionary<string, object> Properties { get; set; }

        [JsonProperty(Order = 9)]
        public List<Test> Children { get; set; }

        public Test() { }

        public Test(string id, string name, string description = null)
        {
            Id = id;
            Name = name;
            Description = description;
            State = StateEnum.Waiting;
            CreatedAt = DateTimeOffset.Now;
        }
    }
}
