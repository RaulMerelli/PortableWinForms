using Android.Webkit;

namespace AndroidWinForms
{
    class ShimWebViewClient : WebViewClient
    {
        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            // Definisco window.chrome.webview.postMessage → bridge.webView2_WebMessageReceived
            view.EvaluateJavascript(@"
                (function(){
                    window.chrome = window.chrome || {};
                    chrome.webview = chrome.webview || {};
                    chrome.webview.postMessage = function(msg){
                        window.bridge.webView2_WebMessageReceived(msg);
                    };
                })();
            ", null);
        }
    }
}
