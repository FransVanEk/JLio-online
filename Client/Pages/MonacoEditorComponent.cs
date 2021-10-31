using System;
using System.Threading.Tasks;
using BlazorMonaco;
using JLioOnline.Client.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace JLioOnline.Client.Pages
{
    public class MonacoEditorComponent : ComponentBase
    {
        public MonacoEditor monacoEditor { get; set; }
        
        [Parameter]
        public MultilineEditorViewModel Model { get; set; }

        public StandaloneEditorConstructionOptions MonacoEditorConstructionOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                Value = "{}",
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
            };
        }

        public async Task EditorOnDidInit(MonacoEditorBase editor)
        {
            await editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, (mEditor, keyCode) =>
            {
                Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
            });

            var newDecorations = new[]
            {
                new ModelDeltaDecoration
                {
                    Range = new BlazorMonaco.Range(3,1,3,1),
                    Options = new ModelDecorationOptions
                    {
                        IsWholeLine = true,
                        ClassName = "decorationContentClass",
                        GlyphMarginClassName = "decorationGlyphMarginClass"
                    }
                }
            };

            decorationIds = await monacoEditor.DeltaDecorations(null, newDecorations);
        }

        private string[] decorationIds;

        protected void OnContextMenu(EditorMouseEvent eventArg)
        {
            Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
        }


        protected async Task OnContentChanged(ModelContentChangedEvent eventArg)
        {
            var value = await monacoEditor.GetValue();
            Model.ScriptText = value;
            Console.WriteLine(value);
        }

        protected async Task ChangeTheme(ChangeEventArgs e)
        {
            Console.WriteLine($"setting theme to: {e.Value}");
            await MonacoEditorBase.SetTheme(e.Value.ToString());
        }
    }
}