using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkGrainInterfaces.Ping;
using BenchmarkGrains.Ping;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Benchmarks.Ping
{
    [MemoryDiagnoser]
    public class LocalSequentialPingBenchmark : IDisposable
    {
        private readonly List<ISiloHost> hosts = new List<ISiloHost>();

        [Benchmark(Baseline = true, Description = "50K Local Msgs", OperationsPerInvoke = 1)]
        public Task SequentialPing() => PingConcurrentHostedClient(blocksPerWorker: 1, maxConcurrency: 1);

        public Task PingConcurrentHostedClient(int blocksPerWorker = 30, int maxConcurrency = 250) => this.Run(
            runs: 1,
            grainFactory: (IGrainFactory)this.hosts[0].Services.GetService(typeof(IGrainFactory)),
            blocksPerWorker: blocksPerWorker,
            maxConcurrency: maxConcurrency);

        private async Task Run(int runs, IGrainFactory grainFactory, int blocksPerWorker, int maxConcurrency = 250)
        {
            var loadGenerator = new ConcurrentLoadGenerator<IPingGrain>(
                maxConcurrency: maxConcurrency,
                blocksPerWorker: blocksPerWorker,
                requestsPerBlock: 50_000,
                issueRequest: g => g.Run(),
                getStateForWorker: workerId => grainFactory.GetGrain<IPingGrain>(Guid.NewGuid().GetHashCode()), false);
            
            while (runs-- > 0) await loadGenerator.Run();
        }


        public async Task Shutdown()
        {
            this.hosts.Reverse();
            foreach (var h in this.hosts)
            {
                await h.StopAsync();
                h.Dispose();
            }
        }

        [GlobalSetup]
        public void IterationSetup()
        {
            var siloBuilder = new SiloHostBuilder()
                .ConfigureDefaults()
                .UseLocalhostClustering(
                    siloPort: 11111,
                    gatewayPort: 30000,
                    primarySiloEndpoint: null);

            siloBuilder.ConfigureApplicationParts(parts =>
                parts.AddApplicationPart(typeof(IPingGrain).Assembly)
                        .AddApplicationPart(typeof(PingGrain).Assembly));


            var silo = siloBuilder.Build();
            silo.StartAsync().GetAwaiter().GetResult();
            this.hosts.Add(silo);
        }

        [GlobalCleanup]
        public void Dispose()
        {
            this.hosts.ForEach(h => h.Dispose());
        }
    }
}