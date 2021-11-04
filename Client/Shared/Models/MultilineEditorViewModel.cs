using System.Collections.Generic;
using JLio.Core.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLioOnline.Client.Shared.Models
{
    public class MultilineEditorViewModel
    {
        [JsonIgnore]
        public List<string> Commands { get; set; } = new List<string>();

        [JsonIgnore]
        public JsonObjectViewModel CurrentInput { get; set; }

        [JsonIgnore]
        public JsonObjectViewModel CurrentOutput { get; set; }

        [JsonIgnore]
        public List<CommandExecutionViewModel> DebugResults { get; set; } = new List<CommandExecutionViewModel>();

        [JsonIgnore]
        public bool HasCurrentInput => CurrentInput != null;

        [JsonIgnore]
        public bool HasCurrentOutput => CurrentOutput != null;

        [JsonProperty("inputObjects")]
        public List<JsonObjectViewModel> InputObjects { get; set; } = new List<JsonObjectViewModel>();

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        public List<JsonObjectViewModel> OutputObjects { get; set; } = new List<JsonObjectViewModel>();

        [JsonProperty("scriptText")]
        public string ScriptText { get; set; } = "";

        [JsonIgnore]
        public string SelectedCommand { get; set; } = "Add";
    }

    public class JsonObjectViewModel
    {
        public string JsonString { get; set; }
        public string Name { get; set; }
    }

    public class CommandExecutionViewModel
    {
        public CommandExecutionViewModel(ICommand command, bool success, JToken startData, JToken endData,
            string logText)
        {
            Command = command;
            Succes = success;
            StartObjectText = startData.ToString(Formatting.Indented);
            EndObjectText = endData.ToString(Formatting.Indented);
            CommandText = GetCommandText(command);
            StartObject = startData.DeepClone();
            EndObject = endData.DeepClone();
            HasChanges = !JToken.DeepEquals(startData, endData);
            LogText = logText;
        }

        public ICommand Command { get; }
        public string CommandText { get; }
        public JToken EndObject { get; }
        public string EndObjectText { get; set; }
        public bool Expanded { get; set; } = false;
        public bool HasChanges { get; }
        public string LogText { get; }
        public JToken StartObject { get; }
        public string StartObjectText { get; set; }
        public bool Succes { get; }

        private string GetCommandText(ICommand command)
        {
            return JsonConvert.SerializeObject(command, Formatting.Indented);
        }
    }
}