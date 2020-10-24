using Nexus.Link.Libraries.Core.Platform.BusinessEvents;

#pragma warning disable 1591

namespace Service.Tests.ContractTests.Capability1.Models
{
    // :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // :::::::::::::::::: TASK: Remove this class ::::::::::::::::::
    // :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    public class Event1
    {
        public Person Person { get; set; }
        public BusinessEventMetaData MetaData { get; set; }
    }

    public class Person
    {
        public string Id { get; set; }
        public string Gender { get; set; }
    }
}