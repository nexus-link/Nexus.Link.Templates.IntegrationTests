using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Service.Tests.ContractTests.Capability1.Models;

#pragma warning disable 1591

namespace Service.Tests.ContractTests.Capability1
{
    public class Capability1RestClient : RestClient
    {
        public Capability1RestClient(IHttpSender httpSender) : base(httpSender)
        {
        }

        public async Task<MockPerson> CreatePerson(MockPerson person)
        {
            var relativeUrl = "PersonManagement/Persons";
            try
            {
                person = await PostAndReturnCreatedObjectAsync(relativeUrl, person);
                return person;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<MockPerson> GetPerson(string id)
        {
            var relativeUrl = $"PersonManagement/Persons/{id}";
            try
            {
                var person = await GetAsync<MockPerson>(relativeUrl);
                return person;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task DeletePerson(string id)
        {
            var relativeUrl = $"PersonManagement/Persons/{id}";
            await DeleteAsync(relativeUrl);
        }

        public async Task<MockOrder> CreateOrder(MockOrder mockOrder)
        {
            var relativeUrl = "OrderManagement/Orders";
            try
            {
                mockOrder = await PostAndReturnCreatedObjectAsync(relativeUrl, mockOrder);
                return mockOrder;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
