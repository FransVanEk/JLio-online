using System;
using System.Threading.Tasks;
using BlazorMonaco;
using Microsoft.AspNetCore.Components;

namespace JLioOnline.Client.Shared.Components
{
    public partial class JsonEditor : ComponentBase
    {
        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public MonacoEditor monacoEditor { get; set; }

        [Parameter]
        public bool ReadOnly { get; set; }

        [Parameter]
        public string Text { get; set; }

        [Parameter]
        public EventCallback<string> TextChanged { get; set; }

        public StandaloneEditorConstructionOptions MonacoEditorConstructionOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                Value = Text,
                Language = "json",
                GlyphMargin = true,
                Scrollbar = new EditorScrollbarOptions
                {
                    Horizontal = "auto",
                    Vertical = "auto"
                },
                AutomaticLayout = true,
                AcceptSuggestionOnCommitCharacter = true,
                LineNumbers = "on",
                ScrollBeyondLastLine = false,
                ReadOnly = ReadOnly
            };
        }

        protected async Task ChangeTheme(ChangeEventArgs e)
        {
            Console.WriteLine($"setting theme to: {e.Value}");
            await MonacoEditorBase.SetTheme(e.Value.ToString());
        }

        private async Task UpdateInput()
        {
            Text = await monacoEditor.GetValue();
            await TextChanged.InvokeAsync(Text);
        }

        public void Update(string text)
        {
            Text = text;
            monacoEditor.SetValue(Text);
        }
    }
}