using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Mapping;

namespace Service.BackgroundTasks
{
    /// <summary>
    /// Cron job for deleting old tests
    /// </summary>
    public class PurgeJob : IHostedService, IDisposable
    {
        private readonly ITestLogic _testLogic;
        private readonly ILogger<PurgeJob> _logger;
        private Timer _timer;

        private static readonly TimeSpan PurgeInterval = TimeSpan.FromDays(1);
        private static readonly TimeSpan MaxAge = TimeSpan.FromDays(7);

        /// <summary></summary>
        public PurgeJob(ITestLogic testLogic, ILogger<PurgeJob> logger)
        {
            _testLogic = testLogic;
            _logger = logger;
        }

        /// <summary></summary>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Purge job starting.");

            _timer = new Timer(async (state) => { await DoWork(state); }, null, TimeSpan.Zero, PurgeInterval);
            
            _logger.LogInformation($"Purge job started with {nameof(PurgeInterval)} = {PurgeInterval} and {nameof(MaxAge)} = {MaxAge}.");
            return Task.CompletedTask;
        }

        private async Task DoWork(object state)
        {
            _logger.LogInformation("Purge work starting...");
            try
            {
                var purgedTests = await _testLogic.PurgeAsync(MaxAge);

                _logger.LogInformation($"Done. Purged {purgedTests} tests.");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error purging: {e.Message}", e);
            }
        }

        /// <summary></summary>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Purge job stopping.");
            
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <summary></summary>
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
