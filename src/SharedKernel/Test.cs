using System;
using System.Collections.Generic;
using System.Linq;
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

        [JsonIgnore]
        public StateEnum InternalState { get; set; }

        [JsonProperty(Order = 7)] public StateEnum State => CalculateStateRecursive(this);

        private StateEnum CalculateStateRecursive(Test test)
        {
            if (test.InternalState != StateEnum.Waiting) return test.InternalState;
            if (test.Children == null || !test.Children.Any()) return test.InternalState;

            var childrenStates = new List<StateEnum>();
            foreach (var child in test.Children)
            {
                var childState = CalculateStateRecursive(child);
                childrenStates.Add(childState);
            }

            if (childrenStates.All(x => x == StateEnum.Ok)) return StateEnum.Ok;
            if (childrenStates.Any(x => x == StateEnum.Failed)) return StateEnum.Failed;
            return StateEnum.Waiting;

            // TODO: When to save calculated states?
        }

        [JsonProperty(Order = 8)]
        public string StateMessage { get; set; }

        [JsonProperty(Order = 9)]
        public IDictionary<string, object> Properties { get; set; }

        [JsonProperty(Order = 10)]
        public List<Test> Children { get; set; }

        public Test() { }

        public Test(string id, string name, string description = null)
        {
            Id = id;
            Name = name;
            Description = description;
            InternalState = StateEnum.Waiting;
            CreatedAt = DateTimeOffset.Now;
        }
    }
}
