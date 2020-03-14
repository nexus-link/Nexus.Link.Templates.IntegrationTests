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
        
        [JsonProperty(Order = 10)]
        public List<Test> Children { get; set; }

        public Test() {}

        public Test(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
