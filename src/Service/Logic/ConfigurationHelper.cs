using Microsoft.Extensions.Configuration;

namespace Service.Logic
{
    /// <summary></summary>
    public class ConfigurationHelper
    {
        private readonly IConfiguration _configuration;

        /// <summary></summary>
        public ConfigurationHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}
