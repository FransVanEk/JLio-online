using System;
using System.Threading.Tasks;
using BlazorMonaco;
using Microsoft.AspNetCore.Components;

namespace JLioOnline.Client.Shared.Components
{
    public partial class JsonDiff : ComponentBase
    {
        private MonacoDiffEditor diffEditor { get; set; }

        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Parameter]
        public string ModifiedJson { get; set; }

        [Parameter]
        public EventCallback<string> ModifiedJsonChanged { get; set; }

        [Parameter]
        public string OriginalJson { get; set; }

        [Parameter]
        public EventCallback<string> OriginalJsonChanged { get; set; }

        [Parameter]
        public bool ShowReload { get; set; } = false;

        private DiffEditorConstructionOptions DiffEditorConstructionOptions(MonacoDiffEditor editor)
        {
            return new DiffEditorConstructionOptions
            {
                OriginalEditable = false,
                InDiffEditor = false,
                EnableSplitViewResizing = true
            };
        }

        public async Task Update()
        {
            await DiffEditorInit(diffEditor);
        }

        private async Task DiffEditorInit(MonacoEditorBase editor)
        {
            var original_model = await MonacoEditorBase.CreateModel(OriginalJson, "json");
            var modified_model = await MonacoEditorBase.CreateModel(ModifiedJson, "json");

            await diffEditor.SetModel(new DiffEditorModel
            {
                Original = original_model,
                Modified = modified_model
            });
        }
    }
}