using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheTestClient
{
    public class ConsoleHostedService : IHostedService
    {
        public ConsoleHostedService()
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
