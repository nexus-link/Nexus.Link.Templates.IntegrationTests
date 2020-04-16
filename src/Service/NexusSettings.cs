using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Service
{
    /// <summary></summary>
    public class NexusSettings
    {
        /// <summary></summary>
        public string ApplicationName { get; set; }
        
        /// <summary></summary>
        public string Organization { get; set; }
        
        /// <summary></summary>
        public string Environment { get; set; }
        
        /// <summary></summary>
        public Tenant Tenant => new Tenant(Organization, Environment);

        /// <summary></summary>
        public string RunTimeLevel { get; set; }

        /// <summary></summary>
        public string PublicKey { get; set; }

        /// <summary></summary>
        public string AuthenticationUrl { get; set; }

        /// <summary></summary>
        public string ClientId { get; set; }

        /// <summary></summary>
        public string ClientSecret { get; set; }

        /// <summary></summary>
        public string BusinessEventsUrl { get; set; }
    }
}
