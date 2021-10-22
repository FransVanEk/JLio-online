using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace JLioOnline.Client.Extensions
{
    public static class SaveAsFile
    {
        public async static Task SaveAs(IJSRuntime js, string filename, byte[] data)
        {
            await js.InvokeAsync<object>(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));
        }
    }
}
