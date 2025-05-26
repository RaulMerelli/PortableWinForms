using Microsoft.UI.Xaml.Controls;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace App1
{
    public static class Page
    {
        static public WebView2 pContainer;
        static internal int defaultLcid;

        public static async Task<string> RunScript(string script)
        {
            string result = await pContainer.ExecuteScriptAsync(script.Replace("\u200B", ""));
            // il carattere \u200B a volte compare e va rimosso per prevenire errori
            return result;
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
        public static async Task<int> GetIntFromScript(string script)
        {
            return int.Parse(JsonValue.Parse(await RunScript(script)).GetString());
        }

        internal static async Task<int> GetLcidFromWebViewAsync()
        {
            if (pContainer?.CoreWebView2 != null)
            {
                try
                {
                    string languageJson = await pContainer.CoreWebView2.ExecuteScriptAsync("navigator.language");
                    string language = languageJson.Trim('"');

                    var culture = new CultureInfo(language);
                    if (culture.IsNeutralCulture)
                    {
                        culture = CultureInfo.CreateSpecificCulture(culture.Name);
                    }

                    return culture.LCID; 
                }
                catch
                {
                    return 1033; // fallback
                }
            }

            return 1033;
        }


    }
}
