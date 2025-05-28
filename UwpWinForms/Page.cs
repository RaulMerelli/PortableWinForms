using Microsoft.UI.Xaml.Controls;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace App1
{
    public static class Page
    {
        static public WebView2 pContainer;
        static internal int defaultLcid;

        public static async Task<string> RunScript(string script)
        {
            string result = await pContainer.ExecuteScriptAsync(script.Replace("\u200B", ""));
            // character \u200B sometimes appear and has to be removed in order to prevent errors
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
            // todo
            throw new NotImplementedException();
        }

        public static async Task<string> Get(string element, string property)
        {
            var raw = await RunScript($"document.getElementById(\"{element}\").{property}");
            // raw is a quoted JSON string, e.g. "\"hello\""
            return JToken.Parse(raw).ToObject<string>();
        }

        public static async Task<string> GetFromScript(string script)
        {
            var raw = await RunScript(script);
            return JToken.Parse(raw).ToObject<string>();
        }

        public static async Task<int> GetIntFromScript(string script)
        {
            var raw = await RunScript(script);
            return JToken.Parse(raw).ToObject<int>();
        }

        internal static async Task<int> GetLcidFromWebViewAsync()
        {
            if (pContainer?.CoreWebView2 != null)
            {
                try
                {
                    var languageJson = await pContainer.CoreWebView2.ExecuteScriptAsync("navigator.language");
                    var language = JToken.Parse(languageJson).ToObject<string>();
                    var culture = new CultureInfo(language);
                    if (culture.IsNeutralCulture)
                        culture = CultureInfo.CreateSpecificCulture(culture.Name);
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
