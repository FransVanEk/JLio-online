using System.Collections.Generic;

namespace JLioOnline.Client.Shared.Models
{
    public class SampleMetadata
    {
        public string Name { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public string Path { get; set; } = string.Empty;
    }
}
