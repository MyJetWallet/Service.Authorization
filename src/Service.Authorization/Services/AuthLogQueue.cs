using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Authorization.Postgres.Models;
using MonitoringLocator = Service.Authorization.Utils.MonitoringLocator;

namespace Service.Authorization.Services
{
    public class AuthLogQueue
    {
        private readonly object _lock = new object();
        private List<IAuthLogModel> Queue = new List<IAuthLogModel>();
        private readonly AuthLogRepository _logRepository;
        private readonly ILogger<AuthLogQueue> _logger;

        public AuthLogQueue(AuthLogRepository logRepository, ILogger<AuthLogQueue> logger)
        {
            _logRepository = logRepository;
            _logger = logger;
        }

        public void HandleEvent(IAuthLogModel auth)
        {
            lock (_lock)
            {
                Queue.Add(auth);
            }

            MonitoringLocator.ItemsInAuthLogQueue.Inc();
            MonitoringLocator.TotalItemsInAuthLogQueue.Inc();
        }

        private IEnumerable<IAuthLogModel> GetEventsAndClearQueue()
        {
            lock (_lock)
            {
                if (!Queue.Any())
                    return Array.Empty<IAuthLogModel>();

                var events = Queue;
                Queue = new List<IAuthLogModel>();
                MonitoringLocator.ItemsInAuthLogQueue.DecTo(0);
                return events;
            }
        }

        private void BackToQueue(IEnumerable<IAuthLogModel> itemsToBack)
        {
            lock (_lock)
            {
                Queue.AddRange(itemsToBack);
                MonitoringLocator.ItemsInAuthLogQueue.Inc(itemsToBack.Count());
                MonitoringLocator.TotalItemsInAuthLogQueue.Inc(itemsToBack.Count());
            }
        }

        private async Task LoopAsync()
        {
            while (true)
            {
                var itemsToInsert = GetEventsAndClearQueue();


                if (!itemsToInsert.Any())
                {
                    await Task.Delay(1000);
                    continue;
                }

                try
                {
                    await _logRepository.Add(itemsToInsert);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                    BackToQueue(itemsToInsert);
                    await Task.Delay(3000);
                }
            }
        }

        public void Start()
        {
            _logger.LogInformation("Auth logs queue was started");
            Task.Run(LoopAsync);
        }
    }
}