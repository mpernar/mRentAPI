using AIForRentersAPI.Controllers;
using AIForRentersAPI.Functionalities;
using AIForRentersAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIForRentersAPI
{
    public class BackgroundProcesses : IHostedService
    {
        private readonly ILogger<BackgroundProcesses> logger;
        private Timer _timer;

        public BackgroundProcesses(ILogger<BackgroundProcesses> logger)
        {
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        public void DoWork(object state)
        {
            List<ReceivedData> receivedData = new List<ReceivedData>();
            receivedData = EmailFetcher.ShapeReceivedData();

            ResponseProcessor.ProcessData(receivedData);

            using (var context = new AIForRentersDbContext())
            {
                List<Request> requests = new List<Request>();

                var query = from req in context.Request.Include("Client")
                            select req;

                requests = query.ToList();

                AvailabilityValidator.CheckForAvailability(requests);

                context.SaveChanges();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
