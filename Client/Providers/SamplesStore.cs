using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JLioOnline.Client.Shared.Models;

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
                var allSamples = new List<Sample>();

                // Define the structure: category -> tag -> sample files
                var categories = new Dictionary<string, string[]>
                {
                    { "commands", new[] { "add", "set", "put", "remove", "copy", "move", "merge", "decisionTable" } },
                    { "functions", new[] { "datetime", "newGuid", "concat", "fetch", "parse", "toString", "partial", "promote", "math", "text", "timedate", "array", "logic", "conversion" } }
                };

                // Load samples from organized folder structure
                foreach (var category in categories)
                {
                    foreach (var tag in category.Value)
                    {
                        var folderPath = $"samples/{category.Key}/{tag}";
                        
                        // Try to load sample files for this tag
                        // We'll try Sample-1 through Sample-100 (more than enough)
                        for (int i = 1; i <= 100; i++)
                        {
                            try
                            {
                                var samplePath = $"{folderPath}/Sample-{i}.json";
                                var sample = await client.GetFromJsonAsync<Sample>(samplePath);
                                if (sample != null)
                                {
                                    allSamples.Add(sample);
                                }
                            }
                            catch
                            {
                                // File doesn't exist or couldn't be loaded, continue
                                continue;
                            }
                        }
                    }
                }

                // Sort samples by their name (Sample-1, Sample-2, etc.)
                allSamples = allSamples
                    .OrderBy(s => int.TryParse(s.Model?.Name?.Replace("Sample-", ""), out int num) ? num : 0)
                    .ToList();

                Samples = new Samples(allSamples.ToArray());
            }

            return Samples;
        }
    }
}