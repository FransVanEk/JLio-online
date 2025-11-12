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

                // Define the structure: category -> tag -> sample file ranges
                // Format: { category, tag, startSample, endSample }
                var sampleRanges = new (string category, string tag, int[] samples)[]
                {
                    // Commands
                    ("commands", "add", new[] { 1, 2, 3, 6, 8, 20, 21, 22, 23, 24, 25, 27, 33, 34, 35 }),
                    ("commands", "set", new[] { 4, 5, 7, 9, 26, 28, 29, 31, 36, 38 }),
                    ("commands", "copy", new[] { 10, 12, 16, 17, 18, 46, 47 }),
                    ("commands", "move", new[] { 11, 13, 14, 15, 19, 48, 49, 50 }),
                    ("commands", "put", new[] { 39, 40, 96, 97, 98 }),
                    ("commands", "remove", new[] { 42, 43, 44, 99, 100 }),
                    ("commands", "merge", new[] { 30, 51, 52, 53, 101 }),
                    ("commands", "decisionTable", new[] { 32, 54, 55, 56, 102 }),
                    ("commands", "ifElse", new[] { 153, 154, 155, 156, 157 }),
                    ("commands", "compare", new[] { 158, 159, 160, 161, 162 }),
                    ("commands", "flatten", new[] { 133, 134, 135, 136, 137 }),
                    ("commands", "resolve", new[] { 138, 139, 140, 141, 142 }),
                    ("commands", "restore", new[] { 143, 144, 145, 146, 147 }),
                    ("commands", "toCsv", new[] { 148, 149, 150, 151, 152 }),
                    
                    // Functions - Core
                    ("functions", "datetime", new[] { 41, 57, 58, 59, 103 }),
                    ("functions", "newGuid", new[] { 60, 61, 62, 108, 109 }),
                    ("functions", "concat", new[] { 37, 45, 63, 64, 65 }),
                    ("functions", "fetch", new[] { 66, 67, 68, 104, 105 }),
                    ("functions", "parse", new[] { 69, 70, 71, 110, 111 }),
                    ("functions", "toString", new[] { 72, 73, 74, 116, 117 }),
                    ("functions", "partial", new[] { 75, 76, 77, 112, 113 }),
                    ("functions", "promote", new[] { 78, 79, 80, 114, 115 }),
                    ("functions", "indirect", new[] { 118, 119, 120, 121, 122 }),
                    ("functions", "scriptPath", new[] { 163, 164, 165, 166, 167 }),
                    
                    // Functions - Extensions
                    ("functions", "math", new[] { 81, 82, 83, 106, 107 }),
                    ("functions", "math-ext", new[] { 178, 179, 180, 181, 182, 183, 184, 185, 186 }),
                    ("functions", "text", new[] { 84, 85, 86, 87, 88, 89 }),
                    ("functions", "timedate", new[] { 90, 91, 92, 93, 94, 95 }),
                    ("functions", "timedate-ext", new[] { 168, 169, 170, 171, 172, 173, 174, 175, 176, 177 })
                };

                // Load samples based on the defined ranges (much faster - only 176 requests instead of 5800)
                foreach (var (category, tag, samples) in sampleRanges)
                {
                    var folderPath = $"samples/{category}/{tag}";
                    
                    foreach (var sampleNum in samples)
                    {
                        try
                        {
                            var samplePath = $"{folderPath}/Sample-{sampleNum}.json";
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