using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Text;

namespace LoadTest
{
    public class Loader
    {
        private string _url;
        private TrackSimulator _trackSimulator;
        private ServiceProvider _serviceProvider;
        private ServiceSourceType _sourceType;
        private String _dataSource;
        public Loader(ServiceProvider serviceProvider,  ServiceSourceType serviceSourceType, String dataSource, String? url)
        {
            _serviceProvider = serviceProvider;
            _url = url;
            _dataSource = dataSource;
            _sourceType = serviceSourceType;
            _trackSimulator = new TrackSimulator();        
        }

        public async Task<string> run(string pId)
        {

            try
            {
                if (_sourceType == ServiceSourceType.API)
                {
                    await runWithAPI(pId);
                }
                else
                {
                    await runWithSelf(pId);
                }
                //update resposne
                var redisClient = _serviceProvider.GetService<RedisClientHelper>();
                await redisClient.Update(pId);
                return pId;
            }catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private async Task runWithAPI(string pId)
        {

            var httpClientHandler = new HttpClientHandler();


            httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using (HttpClient client = new HttpClient(httpClientHandler))
            {
                var bound = _trackSimulator.GetBound();

                var json = JsonConvert.SerializeObject(bound);
                var request = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_url, request);

                var result = await response.Content.ReadAsStringAsync();
                var resultJson = JsonConvert.DeserializeObject<dynamic>(result);
                var data = (JArray)resultJson["data"];

               // Console.WriteLine($"PID: {pId}, executionTime: {resultJson["executionTime"]}, data: {data.Count()}");
            }
        }
        private Task runWithSelf(string pId)
        {
            var dataService = _serviceProvider.GetService<DataService>();
            var bound = _trackSimulator.GetBound();
            if (_dataSource == "db")
            {
                var res = dataService.GetFromDb(bound);
                var data = res.Data.Count();
               // Console.WriteLine($"Db => PID: {pId}, executionTime: {res.ExecutionTime}, data: {data}");
            }
            else
            {
                var res = dataService.GetFromCache(bound);
                var data = res.Data.Count();
               // Console.WriteLine($"Cache => PID: {pId}, executionTime: {res.ExecutionTime}, data: {data}");
            }
            return Task.CompletedTask;
        }
    }
}
