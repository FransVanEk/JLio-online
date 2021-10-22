using JLioOnline.Client.Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JLioOnline.Client.Providers.Models
{
    public class JLioDocumentModel
    {
        public string Id => Name.ToLower().Replace(" ", "_");
        public string Name { get; set; }
        public string Description { get; set; }
        public string JsonData { get; set; }

        public JLioDocumentModel SetJsonData(MultilineEditorViewModel data)
        {
            JsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            return this;
        }
    }
}
