using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Service.BackgroundTasks;
using FulcrumApplicationHelper = Nexus.Link.Libraries.Web.AspNet.Application.FulcrumApplicationHelper;

namespace Service
{
    /// <summary></summary>
    public class Program
    {
        /// <summary></summary>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary></summary>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((context, services) =>
                {
                    var nexusSettings = context.Configuration.GetSection("Nexus");
                    FulcrumApplicationHelper.WebBasicSetup(nexusSettings);

                    services.AddHostedService<PurgeJob>();
                })
                .ConfigureLogging((context, loggingBuilder) =>
                {
                    FulcrumApplication.Setup.SynchronousFastLogger = new DebugTraceLogger(); // TODO
                });
    }
}
