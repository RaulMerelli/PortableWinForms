using Android.Webkit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Media.Session.MediaSession;
using Newtonsoft.Json.Linq;
using Android.App;

namespace AndroidWinForms
{
    public static class Page
    {
        static WebView webView => MainActivity.Instance?.WebView;

        static internal int defaultLcid;

        public static async Task<string> RunScript(string script)
        {
            var tcs = new TaskCompletionSource<string>();
            webView.EvaluateJavascript(script.Replace("\u200B", ""), new ValueCallback(result =>
            {
                // result è una stringa JSON-escaped, es. "\"hello\""
                tcs.SetResult(result);
            }));
            return await tcs.Task;
        }

        public static Task Set(string element, string property, string value) =>
            RunScript($"document.getElementById('{element}').{property}={value};");

        public static Task Add(string element, string property, string value) =>
            RunScript($"document.getElementById('{element}').{property}+={value};");

        public static Task Remove(string element, string property) =>
            throw new System.NotImplementedException();

        public static async Task<string> Get(string element, string property)
        {
            string json = await RunScript($"document.getElementById('{element}').{property};");
            // rimuovo eventuali virgolette esterne
            return JToken.Parse(json).ToString();
        }

        public static async Task<string> GetFromScript(string script)
        {
            string json = await RunScript(script);
            return JToken.Parse(json).ToString();
        }

        public static async Task<int> GetIntFromScript(string script)
        {
            string str = await GetFromScript(script);
            return int.Parse(str, CultureInfo.InvariantCulture);
        }

        public static Task<int> GetLcidFromWebViewAsync()
        {
            // su Android prendo la locale di sistema
            var locale = Java.Util.Locale.Default;
            var culture = new CultureInfo(locale.ToLanguageTag());
            if (culture.IsNeutralCulture)
                culture = CultureInfo.CreateSpecificCulture(culture.Name);
            return Task.FromResult(culture.LCID);
        }

        // callback wrapper per EvaluateJavascript
        class ValueCallback : Java.Lang.Object, IValueCallback
        {
            readonly System.Action<string> _onReceive;
            public ValueCallback(System.Action<string> onReceive) => _onReceive = onReceive;
            public void OnReceiveValue(Java.Lang.Object value) =>
                _onReceive?.Invoke(value?.ToString() ?? "");
        }
    }
}
