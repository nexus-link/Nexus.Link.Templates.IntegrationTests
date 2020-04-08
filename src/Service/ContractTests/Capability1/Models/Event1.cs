#pragma warning disable 1591

namespace Service.ContractTests.Capability1.Models
{
    public class Event1
    {
        public Person Person { get; set; }
    }

    public class Person
    {
        public string Id { get; set; }
        public string Gender { get; set; }
    }
}