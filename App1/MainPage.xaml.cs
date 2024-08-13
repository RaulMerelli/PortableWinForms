using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using Windows.Storage;
using winformtest;

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
            Page.pContainer = WebView2;
            await Page.pContainer.EnsureCoreWebView2Async();
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            Page.pContainer.CoreWebView2.Navigate(Path.Combine("file:", appInstalledFolder.Path, @"html\", "index.html"));
            Page.pContainer.NavigationCompleted += PContainer_NavigationCompleted;
        }

        async private void PContainer_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (args != null)
            {
                if (args.IsSuccess)
                {
                    //Application.EnableVisualStyles();
                    //Application.SetCompatibleTextRenderingDefault(false);
                    new Form1();
                    await Page.pContainer.CoreWebView2.Profile.ClearBrowsingDataAsync();
                }
            }
        }
    }
}
