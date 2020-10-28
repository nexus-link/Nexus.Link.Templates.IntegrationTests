using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dashboard.Logic
{
    public class TestLogic : ITestLogic
    {
        private readonly HttpClient _httpClient;

        public TestLogic(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TestServiceClient");
        }

        public async Task<JsonElement> GetTestAsync(string testId)
        {
            var response = await _httpClient.GetAsync($"api/v1/Tests/{WebUtility.UrlEncode(testId)}");
            var resultString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception($"Error when trying to start test batch. {response.StatusCode}: {resultString}");
            var test = JsonSerializer.Deserialize<JsonElement>(resultString);
            return test;
        }

        public async Task<JsonElement> CreateTest()
        {
            var response = await _httpClient.PostAsync("api/v1/AllTests", null);
            var resultString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception($"Error when trying to start test batch. {response.StatusCode}: {resultString}");
            var test = JsonSerializer.Deserialize<JsonElement>(resultString);
            return test;
        }
    }
}
