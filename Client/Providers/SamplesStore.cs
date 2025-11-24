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
        private List<SampleMetadata>? _sampleMetadataCache;
        private readonly Dictionary<int, Sample> _sampleCache = new();

        public SamplesStore(HttpClient client)
        {
            this.client = client;
        }

        public Samples Samples { get; set; } = new Samples();

        /// <summary>
        /// Clears all cached data - useful when samples are updated during development
        /// </summary>
        public void ClearCache()
        {
            _sampleMetadataCache = null;
            _sampleCache.Clear();
            Samples = new Samples();
        }

        /// <summary>
        /// Gets the list of all sample metadata (lightweight, just titles and tags)
        /// </summary>
        public async Task<List<SampleMetadata>> GetSampleMetadataAsync()
        {
            if (_sampleMetadataCache != null)
            {
                return _sampleMetadataCache;
            }

            try
            {
                // Add cache busting for development
                var cacheBuster = DateTime.UtcNow.Ticks.ToString();
                var url = $"samples/samples-index.json?v={cacheBuster}";
                var metadata = await client.GetFromJsonAsync<List<SampleMetadata>>(url);
                _sampleMetadataCache = metadata ?? new List<SampleMetadata>();
                return _sampleMetadataCache;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sample metadata: {ex.Message}");
                return new List<SampleMetadata>();
            }
        }

        /// <summary>
        /// Gets a specific sample's full content by its number
        /// </summary>
        public async Task<Sample?> GetSampleAsync(int sampleNumber)
        {
            // Check cache first
            if (_sampleCache.ContainsKey(sampleNumber))
            {
                return _sampleCache[sampleNumber];
            }

            var metadata = await GetSampleMetadataAsync();
            var sampleMeta = metadata.FirstOrDefault(m => m.Number == sampleNumber);
            
            if (sampleMeta == null)
            {
                return null;
            }

            try
            {
                // Add cache busting for development
                var cacheBuster = DateTime.UtcNow.Ticks.ToString();
                var url = $"samples/{sampleMeta.Path}?v={cacheBuster}";
                var sample = await client.GetFromJsonAsync<Sample>(url);
                
                if (sample != null)
                {
                    _sampleCache[sampleNumber] = sample;
                }
                
                return sample;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sample {sampleNumber}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Legacy method - loads all samples at once (expensive!)
        /// Consider using GetSampleMetadataAsync() + GetSampleAsync() instead
        /// </summary>
        public async Task<Samples> GetSamples()
        {
            if (Samples == null || !Samples.Any())
            {
                var allSamples = new List<Sample>();
                var metadata = await GetSampleMetadataAsync();

                // Load each sample (this is expensive - only use when you need ALL samples)
                foreach (var meta in metadata)
                {
                    try
                    {
                        var sample = await GetSampleAsync(meta.Number);
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

                Samples = new Samples(allSamples.ToArray());
            }

            return Samples;
        }
    }
}