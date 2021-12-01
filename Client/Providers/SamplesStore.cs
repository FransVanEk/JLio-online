using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JLioOnline.Client.Shared.Models;
using Newtonsoft.Json;

namespace JLioOnline.Client.Providers
{
    public class SamplesStore
    {
        private readonly HttpClient client;

        public SamplesStore(HttpClient client)
        {
            this.client = client;
        }

        public Samples Samples { get; set; } = new Samples();

        public async Task<Samples> GetSamples()
        {
            if (Samples == null || !Samples.Any())
            {
               var result = await client.GetFromJsonAsync<Sample[]>("samples/samples.json");
                Samples = new Samples(result);
            }
            return Samples;
        }
    }
}