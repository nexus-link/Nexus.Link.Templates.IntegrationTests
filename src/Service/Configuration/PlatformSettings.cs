namespace Service.Configuration
{
    /// <summary></summary>
    public class PlatformSettings
    {
        /// <summary>The base url to the Business API</summary>
        public string BusinessApiUrl { get; set; }

        /// <summary>The base url to the Integration API, where such things as event publishing, token creation and value association are</summary>
        public string IntegrationApiUrl { get; set; }

        /// <summary>Credentials for accessing components within the customer's platform</summary>
        public string ClientId { get; set; }

        /// <summary>Credentials for accessing components within the customer's platform</summary>
        public string ClientSecret { get; set; }
    }
}
