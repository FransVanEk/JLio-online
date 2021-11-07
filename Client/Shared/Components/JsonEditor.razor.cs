using BlazorMonaco;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace JLioOnline.Client.Shared.Components
{
    public partial class JsonEditor : ComponentBase
    {
        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Parameter]
        public string  Text { get; set; }

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
                ScrollBeyondLastLine = false
            };
        }

        public MonacoEditor monacoEditor { get; set; }

        protected async Task ChangeTheme(ChangeEventArgs e)
        {
            Console.WriteLine($"setting theme to: {e.Value}");
            await MonacoEditorBase.SetTheme(e.Value.ToString());
        }

        async Task UpdateInput()
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
