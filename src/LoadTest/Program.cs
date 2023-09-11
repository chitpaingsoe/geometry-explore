using Geomertry.Models;
using LoadTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;

var arg = Environment.GetCommandLineArgs();

var count = 1;
var dataSource = "db";
var serviceSource = ServiceSourceType.API;

if(arg.Length > 1)
{
    count= int.Parse(arg[3]);
    dataSource = arg[2];
    var srvSource = arg[1];
    if (srvSource == "api")
    {
        serviceSource = ServiceSourceType.API;
    }
    else
    {
        serviceSource = ServiceSourceType.SELF;
    }
}


var url = "http://localhost:7063/api/bus";
if(dataSource == "db")
{
    url += "/search";
}
else
{
    url += "/searchInCache";
}

Console.WriteLine($"service: {serviceSource}, process_count: {count}, ApiUrl: {url}");

// service
var services = new ServiceCollection();

// cache
var conn = ConnectionMultiplexer.Connect("localhost:6379");
Console.WriteLine($"{conn.IsConnected}");
var db = conn.GetDatabase();
services.AddSingleton<IDatabase>(db);

// db
var connectionString = "Data Source=localhost;initial catalog=LTA;persist security info=True;Integrated Security=true;MultipleActiveResultSets=True;App=EntityFramework;TrustServerCertificate=True";
services.AddDbContext<LTAContext>(options =>
options.UseSqlServer(connectionString, x => x.UseNetTopologySuite().MigrationsAssembly("LoadTest")));

// DI
services.AddSingleton<DataService>();
services.AddSingleton<RedisClientHelper>();

// create service Provider
ServiceProvider serviceProvider = services.BuildServiceProvider();


var processes = Enumerable.Range(0, count);

ConcurrentDictionary<string, int> batches = new ConcurrentDictionary<string, int>();

var redisClient = serviceProvider.GetService<RedisClientHelper>();

Task.Run(async () =>
{

    while (true)
    {
        Console.Clear();
        Console.WriteLine($"*** Summary ( 1 batch = {count} per requests) ***");
        foreach (var batchId in batches.Keys)
        {
            var remaingCount = await redisClient.Get(batchId);
            Console.WriteLine($"BatchId: {batchId}, left: {remaingCount}");
            if(int.Parse(remaingCount) == 0)
            {
                var value = 0;
                batches.Remove(batchId, out value);
                await redisClient.Delete(batchId);
            }
        }
        Task.Delay(1000).Wait();
    }
});

while (true)
{
    try
    {
        var processId = Guid.NewGuid().ToString();
        var tasks = new List<Task<string>>();

        await redisClient.Add(processId, count);

        processes.ToList().ForEach(x => {
            tasks.Add(new Loader(serviceProvider, serviceSource, dataSource, url).run(processId));
        });

        batches.TryAdd(processId, count);

        await Task.WhenAll(tasks);

        Task.Delay(1000).Wait();
    }catch(Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}