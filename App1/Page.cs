using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace App1
{
    public static class Page
    {
        static public WebView2 pContainer;

        public static async Task<string> RunScript(string script)
        {
            // il carattere \u200B a volte compare e va rimosso per prevenire errori
            return await pContainer.ExecuteScriptAsync(script.Replace("\u200B", ""));
        }

        public static async Task Set(string element, string property, string value)
        {
            await RunScript($"document.getElementById(\"{element}\").{property}={value}");
        }

        public static async Task Add(string element, string property, string value)
        {
            await RunScript($"document.getElementById(\"{element}\").{property}+={value}");
        }

        public static async Task Remove(string element, string property)
        {
            // Da fare
            throw new NotImplementedException();
        }

        public static async Task<string> Get(string element, string property)
        {
            return JsonValue.Parse(await RunScript($"document.getElementById(\"{element}\").{property}")).GetString();
        }
        public static async Task<string> GetFromScript(string script)
        {
            return JsonValue.Parse(await RunScript(script)).GetString();
        }
    }
}
