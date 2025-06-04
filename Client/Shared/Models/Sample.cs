using System.Collections.Generic;
using Newtonsoft.Json;

namespace JLioOnline.Client.Shared.Models
{
    public class Sample
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("model")]
        public MultilineEditorViewModel Model { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}