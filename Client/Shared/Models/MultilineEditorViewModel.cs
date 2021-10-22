using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace JLioOnline.Client.Shared.Models
{
    public class MultilineEditorViewModel
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public string  SelectedCommand { get; set; } = "Add";

        [JsonIgnore]
        public  List<string> Commands { get; set; }  = new List<string>();

        [JsonProperty("inputObjects")]
        public List<JsonObjectViewModel> InputObjects { get; set; } = new List<JsonObjectViewModel>();

        [JsonIgnore]
        public List<CommandExecutionViewModel> DebugResults { get; set; } = new List<CommandExecutionViewModel>();
        public List<JsonObjectViewModel> OutputObjects { get; set; } = new List<JsonObjectViewModel>();

        [JsonIgnore]
        public JsonObjectViewModel CurrentInput { get; set; }

        [JsonIgnore]
        public JsonObjectViewModel CurrentOutput { get; set; }

        [JsonIgnore]
        public bool HasCurrentInput => CurrentInput != null;

        [JsonIgnore]
        public bool HasCurrentOutput => CurrentOutput != null;

        [JsonProperty("scriptText")]
        public string ScriptText { get; set; } = "";
       
    }

    public class JsonObjectViewModel
    {
        public string Name { get; set; }
        public string JsonString { get; set; }  
    }

    public class CommandExecutionViewModel
    {

        public CommandExecutionViewModel(ICommand command, bool success, JToken startData, JToken endData, string logText)
        {
            Command = command;
            Succes = success;
            StartObjectText = startData.ToString(Formatting.Indented);
            EndObjectText = endData.ToString(Formatting.Indented);
            CommandText =  GetCommandText(command);
            StartObject = startData.DeepClone();
            EndObject = endData.DeepClone();
            HasChanges = !JToken.DeepEquals(startData, endData);
            LogText = logText;
        }

        public ICommand Command { get;  } 
        public bool Succes { get; }
        public string StartObjectText { get; }
        public string EndObjectText { get; }
        public JToken StartObject { get; }
        public JToken EndObject { get;  }
        public bool HasChanges { get; }
        public string LogText { get; }
        public string CommandText { get;  }
        private string GetCommandText(ICommand command)
        {
            return JsonConvert.SerializeObject(command, Formatting.Indented);
        }
        public bool Expanded { get; set; } = false;
    }
}
