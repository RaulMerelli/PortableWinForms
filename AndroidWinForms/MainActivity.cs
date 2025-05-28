using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.WebKit;
using Java.Interop;
using System;
using System.Reflection;

namespace AndroidWinForms;

[Activity(Label = "@string/app_name", MainLauncher = true,
           Theme = "@style/TransparentWindow", Exported = true,
           ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation |
                                  Android.Content.PM.ConfigChanges.ScreenSize)]
public class MainActivity : Activity
{
    const int REQUEST_CODE_OVERLAY = 1234;
    private IWindowManager windowManager;
    private View floatingView;
    public static MainActivity Instance { get; private set; }
    private bool isOverlayCreated = false;

    WebView webView;
    public WebView WebView => webView;

    WebViewAssetLoader assetLoader;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        //RequestWindowFeature(WindowFeatures.NoTitle);
        base.OnCreate(savedInstanceState);

        Instance = this;

        // Controlla permesso overlay
        if (!CheckOverlayPermission())
        {
            return;
        }

        // Imposta trasparenza
        Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
        Window.AddFlags(WindowManagerFlags.LayoutInScreen |
                        WindowManagerFlags.NotFocusable);
        // Crea e mostra overlay con la WebView
        CreateOverlay();
    }

    private bool CheckOverlayPermission()
    {
        if (!Settings.CanDrawOverlays(this))
        {
            var intent = new Intent(
                Settings.ActionManageOverlayPermission,
                Android.Net.Uri.Parse("package:" + PackageName)
            );
            StartActivityForResult(intent, REQUEST_CODE_OVERLAY);
            return false;
        }
        return true;
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        if (requestCode == REQUEST_CODE_OVERLAY)
        {
            if (Settings.CanDrawOverlays(this))
            {
                CreateOverlay();
            }
            else
            {
                Toast.MakeText(this, "Permesso overlay richiesto!", ToastLength.Long).Show();
                Finish();
            }
        }
        base.OnActivityResult(requestCode, resultCode, data);
    }

    private void CreateOverlay()
    {
        // Ottieni WindowManager
        windowManager = GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

        // Inflates il layout con WebView
        // Inflates layout; ensure you have a WebView with id 'webview' in this layout
        floatingView = LayoutInflater.Inflate(Resource.Layout.activity_main, null, false)
            ?? throw new NullReferenceException("Failed to inflate layout Resource.Layout.activity_main");

        // Configura WebView dentro floatingView
        webView = floatingView.FindViewById<WebView>(Resource.Id.webview)
                 ?? throw new NullReferenceException("WebView not found in layout");
        webView.Settings.JavaScriptEnabled = true;
        webView.Settings.DomStorageEnabled = true;
        webView.SetWebChromeClient(new WebChromeClient());
        webView.AddJavascriptInterface(this, "bridge");
        webView.Settings.AllowUniversalAccessFromFileURLs = true;
        webView.SetBackgroundColor(Color.Transparent);
        webView.SetLayerType(LayerType.Software, null);

        assetLoader = new WebViewAssetLoader.Builder()
            .SetDomain("appassets.example")
            .AddPathHandler("/", new WebViewAssetLoader.AssetsPathHandler(this))
            .Build();

        webView.SetWebViewClient(new LocalContentWebViewClient(assetLoader));
        webView.LoadUrl("https://appassets.example/html/index.html");

        // Parametri overlay
        var layoutParams = new WindowManagerLayoutParams(
            WindowManagerLayoutParams.MatchParent,
            WindowManagerLayoutParams.MatchParent,
            (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                ? WindowManagerTypes.ApplicationOverlay
                : WindowManagerTypes.Phone,
            WindowManagerFlags.NotFocusable |
            WindowManagerFlags.LayoutInScreen |
            WindowManagerFlags.HardwareAccelerated, // Migliora performance
            Format.Translucent)
        {
            Gravity = GravityFlags.Top | GravityFlags.Start,
            X = 0,
            Y = 0
        };

        // Aggiunge overlay
        windowManager.AddView(floatingView, layoutParams);
        isOverlayCreated = true;
    }

    private void RemoveOverlay()
    {
        if (floatingView != null)
        {
            windowManager?.RemoveView(floatingView);
            webView?.Destroy(); // Distrugge esplicitamente la WebView
            floatingView = null;
            isOverlayCreated = false;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        RemoveOverlay();
        Instance = null;
    }

    [JavascriptInterface]
    [Export("webView2_WebMessageReceived_Bridge")]
    public void webView2_WebMessageReceived_Bridge(string json)
    {
        Instance.RunOnUiThread(() =>
        {
            foreach (var handler in WebViewHandlerRegistry.Instances)
            {
                var method = handler.GetType()
                    .GetMethod("webView2_WebMessageReceived", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                method?.Invoke(handler, new object[] { json });
            }
        });
    }

    class LocalContentWebViewClient : WebViewClient
    {
        readonly WebViewAssetLoader loader;
        public LocalContentWebViewClient(WebViewAssetLoader loader) => this.loader = loader;

        public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
            => loader.ShouldInterceptRequest(request.Url) ?? base.ShouldInterceptRequest(view, request);

        public async override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            view.EvaluateJavascript(@"
                window.chrome = window.chrome || {};
                chrome.webview = chrome.webview || {};
                chrome.webview.postMessage = function(msg){
                    if (window.bridge && typeof window.bridge.webView2_WebMessageReceived_Bridge === 'function') {
                        window.bridge.webView2_WebMessageReceived_Bridge(msg);
                    } else {
                        alert('[shim] bridge not defined or wrong type');
                    }
                };
            ", null);

            Page.defaultLcid = await Page.GetLcidFromWebViewAsync();
            winformtest.Program.InternalMain();
        }
    }
}
