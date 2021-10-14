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
        public List<JsonObjectViewModel> InputObjects { get; set; } = new List<JsonObjectViewModel>();
        public List<CommandExecutionViewModel> DubugResults { get; set; } = new List<CommandExecutionViewModel>();
        public List<JsonObjectViewModel> OutputObjects { get; set; } = new List<JsonObjectViewModel>();
        public JsonObjectViewModel CurrentInput { get; set; }
        public JsonObjectViewModel CurrentOutput { get; set; }
        public bool HasCurrentInput => CurrentInput != null;
        public bool HasCurrentOutput => CurrentOutput != null;
        public string ScriptText {get;set;}
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
        public ICommand command { get; set; } 
        public bool Succes { get; set; }
        public string StartObject { get; set; }
        public string EndObject { get; set; }

        public string CommandText => GetCommandText();

        private string GetCommandText()
        {
            return JsonConvert.SerializeObject(command, Formatting.Indented);
        }
        bool expended { get; set; } = false;
    }
}
