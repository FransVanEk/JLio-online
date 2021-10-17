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
        public string  SelectedCommand { get; set; } = "Add";
        public int displayMode { get; set; } = 1;

        public readonly List<string> Commands = new List<string>() { "Add", "Copy", "Move", "Remove", "Set" };
        public List<JsonObjectViewModel> InputObjects { get; set; } = new List<JsonObjectViewModel>();
        public List<CommandExecutionViewModel> DubugResults { get; set; } = new List<CommandExecutionViewModel>();
        public List<JsonObjectViewModel> OutputObjects { get; set; } = new List<JsonObjectViewModel>();
        public JsonObjectViewModel CurrentInput { get; set; }
        public JsonObjectViewModel CurrentOutput { get; set; }
        public bool HasCurrentInput => CurrentInput != null;
        public bool HasCurrentOutput => CurrentOutput != null;
        public string ScriptText { get; set; } = "";
        public eMode Mode { get; set; }

        public enum eMode
        {
            script,
            advanced,
            debug
        }
    }

    public class JsonObjectViewModel
    {
        public string Name { get; set; }
        public string JsonString { get; set; }  
    }

    public class CommandExecutionViewModel
    {

        public CommandExecutionViewModel(ICommand command, bool success, JToken startData, JToken endData)
        {
            Command = command;
            Succes = success;
            StartObjectText = startData.ToString(Formatting.Indented);
            EndObjectText = endData.ToString(Formatting.Indented);
            CommandText =  GetCommandText(command);
            StartObject = startData.DeepClone();
            EndObject = endData.DeepClone();
            HasChanges = JToken.DeepEquals(startData, endData);
        }

        public ICommand Command { get;  } 
        public bool Succes { get; }
        public string StartObjectText { get; }
        public string EndObjectText { get; }
        public JToken StartObject { get; }
        public JToken EndObject { get;  }
        public bool HasChanges { get; }
        public string CommandText { get;  }
        private string GetCommandText(ICommand command)
        {
            return JsonConvert.SerializeObject(command, Formatting.Indented);
        }
        public bool Expanded { get; set; } = false;
    }
}
