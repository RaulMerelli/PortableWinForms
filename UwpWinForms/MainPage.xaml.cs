using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;

namespace App1
{
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        public MainPage()
        {
            InitializeComponent();
            InitwebView();
        }

        async void InitwebView()
        {
            bool result = Windows.UI.ViewManagement.ApplicationViewScaling.TrySetDisableLayoutScaling(true);
            Page.pContainer = WebView2;
            await Page.pContainer.EnsureCoreWebView2Async();
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            string page = "https://appassets.example/index.html";
            Page.pContainer.CoreWebView2.SetVirtualHostNameToFolderMapping("appassets.example", Path.Combine("file:", appInstalledFolder.Path, @"html"), CoreWebView2HostResourceAccessKind.Allow);
            Page.pContainer.Source = new Uri(page);
            Page.pContainer.NavigationCompleted += PContainer_NavigationCompleted;
        }

        async private void PContainer_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (args != null)
            {
                if (!args.IsSuccess)
                {
                    Debug.WriteLine($"Navigation failed: Error code {args.WebErrorStatus}");
                }
                if (args.IsSuccess)
                {
                    Page.defaultLcid = await Page.GetLcidFromWebViewAsync();
                    winformtest.Program.InternalMain();
                    await Page.pContainer.CoreWebView2.Profile.ClearBrowsingDataAsync();
                }
            }
        }
    }
}
