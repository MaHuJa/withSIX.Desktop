// <copyright company="SIX Networks GmbH" file="SixWebControlBehavior.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core;

namespace SN.withSIX.Play.Presentation.Wpf.Behaviors
{
    public class RequestHandler : IRequestHandler
    {
        public static readonly string VersionNumberString = String.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}",
            Cef.ChromiumVersion, Cef.CefVersion, Cef.CefSharpVersion);

        bool IRequestHandler.OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            bool isRedirect) {
            if (!frame.IsMain || CommonUrls.IsWithSixUrl(request.Url) || IsAuthUrl(new Uri(request.Url)))
                return false;
            OpenInSystemBrowser(request.Url);
            return true;
        }

        bool IRequestHandler.OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) {
            return OnOpenUrlFromTab(browserControl, browser, frame, targetUrl, targetDisposition, userGesture);
        }

        bool IRequestHandler.OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode,
            string requestUrl, ISslInfo sslInfo, IRequestCallback callback) {
            //NOTE: If you do not wish to implement this method returning false is the default behaviour
            // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
            //callback.Dispose();
            //return false;
            //NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            if (!callback.IsDisposed) {
                using (callback) {
                    //To allow certificate
                    //callback.Continue(true);
                    //return true;
                }
            }
            return false;
        }

        void IRequestHandler.OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath) {
            // TODO: Add your own code here for handling scenarios where a plugin crashed, for one reason or another.
        }

        CefReturnValue IRequestHandler.OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request, IRequestCallback callback) {
            if (CommonUrls.IsWithSixUrl(request.Url)) {
                var headers = request.Headers;
                headers[Common.ClientHeader] = DomainEvilGlobal.SecretData.UserInfo.ClientId.ToString();
                headers[Common.ClientHeaderV] = Common.App.ProductVersion;
                request.Headers = headers;
            }

            return CefReturnValue.Continue;
            //Example of how to set Referer
            // Same should work when setting any header
            // For this example only set Referer when using our custom scheme
            var url = new Uri(request.Url);
            if (url.Scheme == "customscheme") // CefSharpSchemeHandlerFactory.SchemeName
            {
                var headers = request.Headers;
                headers["Referer"] = "http://google.com";

                request.Headers = headers;
            }

            //NOTE: If you do not wish to implement this method returning false is the default behaviour
            // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
            //callback.Dispose();
            //return false;

            //NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            if (!callback.IsDisposed) {
                using (callback) {
                    if (request.Method == "POST") {
                        using (var postData = request.PostData) {
                            if (postData != null) {
                                var elements = postData.Elements;

                                var charSet = request.GetCharSet();

                                foreach (var element in elements) {
                                    if (element.Type == PostDataElementType.Bytes) {
                                        var body = element.GetBody(charSet);
                                    }
                                }
                            }
                        }
                    }

                    //Note to Redirect simply set the request Url
                    //if (request.Url.StartsWith("https://www.google.com", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    request.Url = "https://github.com/";
                    //}

                    //Callback in async fashion
                    //callback.Continue(true);
                    //return CefReturnValue.ContinueAsync;
                }
            }

            return CefReturnValue.Continue;
        }

        bool IRequestHandler.GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy,
            string host, int port, string realm, string scheme, IAuthCallback callback) {
            //NOTE: If you do not wish to implement this method returning false is the default behaviour
            // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.

            callback.Dispose();
            return false;
        }

        public bool OnBeforePluginLoad(IWebBrowser browserControl, IBrowser browser, string url, string policyUrl,
            WebPluginInfo info) {
            return true;
        }

        void IRequestHandler.OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser,
            CefTerminationStatus status) {
            // TODO: Add your own code here for handling scenarios where the Render Process terminated for one reason or another.
            //browserControl.Load(CefExample.RenderProcessCrashedUrl);
        }

        bool IRequestHandler.OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize,
            IRequestCallback callback) {
            //NOTE: If you do not wish to implement this method returning false is the default behaviour
            // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
            //callback.Dispose();
            //return false;

            //NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            if (!callback.IsDisposed) {
                using (callback) {
                    //Accept Request to raise Quota
                    //callback.Continue(true);
                    //return true;
                }
            }

            return false;
        }

        void IRequestHandler.OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request, ref string newUrl) {
            //Example of how to redirect - need to check `newUrl` in the second pass
            //if (string.Equals(frame.GetUrl(), "https://www.google.com/", StringComparison.OrdinalIgnoreCase) && !newUrl.Contains("github"))
            //{
            //	newUrl = "https://github.com";
            //}
        }

        bool IRequestHandler.OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url) {
            return url.StartsWith("mailto");
        }

        void IRequestHandler.OnRenderViewReady(IWebBrowser browserControl, IBrowser browser) {}
        public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response) {
            return false;
        }

        static void OpenInSystemBrowser(string targetURL) {
            Tools.Generic.TryOpenUrl(targetURL);
        }

        static bool IsAuthUrl(Uri uri) {
            var url = uri.ToString();
            return uri.Host.StartsWith("accounts.google.")
                   || url.Contains("steamcommunity.com/openid/login");
        }

        protected virtual bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) {
            return false;
        }
    }

    public class SixWebControlBehavior : DependencyObject
    {
        static void WebControlOnInitialized(object s,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var webControl = (ChromiumWebBrowser) s;
            //webControl.ShowCreatedWebView += OnWebControlOnShowCreatedWebView;

            webControl.ConsoleMessage += OnConsoleMessage;

            // DocumentReady does not signify JS loaded, LoadingFrameComplete should.
            webControl.FrameLoadEnd += WebControlOnLoadingFrameComplete;

            //if (webControl.ViewType == WebViewType.Window)
            //webControl.LoginRequest += WebControl_LoginRequest;

            webControl.RequestHandler = new RequestHandler();

            //var navigationInterceptor =
            //(INavigationInterceptor) webControl.GetService(typeof (INavigationInterceptor));
            //navigationInterceptor.BeginNavigation += NavigationInterceptorOnBeginNavigation;
        }

        static void OnConsoleMessage(object sender, ConsoleMessageEventArgs e) {
            MainLog.Logger.Info("CEF: " + e.Message  + " [" + e.Source + ":" + e.Line + "]");
        }

        /*        static void WebControl_LoginRequest(object sender, LoginRequestEventArgs e) {
            // We are on UI thread here, so we can't use MetroDialog for this..
            var info =
                new WpfDialogManager(new CustomWindowManager()).UserNamePasswordDialog(
                    "Please enter username and password", e.Info.RequestURL);
            if (!info.Item3.GetValueOrDefault()) {
                e.Cancel = true;
                return;
            }
            e.Username = info.Item1;
            e.Password = info.Item2;
            e.Handled = EventHandling.Modal;
        }*/

        /*        static void WebControlOnLoadingFrameFailed(object sender,
            LoadingFrameFailedEventArgs loadingFrameFailedEventArgs) {
            var wb = (WebControl) sender;
            if (loadingFrameFailedEventArgs.IsMainFrame && loadingFrameFailedEventArgs.ErrorCode != NetError.ABORTED) {
                wb.LoadHTML(GetFailedHtml(loadingFrameFailedEventArgs.ErrorCode,
                    loadingFrameFailedEventArgs.ErrorDescription));
            }
        }

        static string GetFailedHtml(NetError errorCode, string errorDescription) {
            return
                string.Format(
                    "<html><head></head><body><div><h3>Failed loading, please confirm internet connected</h3></div><br /><p>Code: {0}<br />Description: {1}</p></body></html>",
                    errorCode, errorDescription);
        }*/

        static void WebControlOnLoadingFrameComplete(object sender, FrameLoadEndEventArgs frameLoadEndEventArgs) {
            // Executing JS in about:blank will lock up the browser

            if (!frameLoadEndEventArgs.Frame.IsMain || frameLoadEndEventArgs.Url == "about:blank")
                return;

            var webControl = (ChromiumWebBrowser) sender;
            UiHelper.TryOnUiThread(() => {
                if (GetEnableAutoHeight(webControl))
                    SetHeightFromDocument(webControl);
            });
        }

        /*
        static void OnWebControlOnShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs args) {
            if (args.IsNavigationCanceled) {
                args.Cancel = true;
                OpenInSystemBrowser(args.TargetURL);
                return;
            }

            var control = (WebControl) sender;
            if (CommonUrls.IsWithSixUrl(args.TargetURL) || IsAuthUrl(args.TargetURL)) {
                if (args.IsWindowOpen || args.IsPost)
                    control.Dispatcher.BeginInvoke(new Action(() => CreateBrowserView(args.NewViewInstance).ShowDialog()));
                else {
                    control.Dispatcher.BeginInvoke(new Action(() => CreateBrowserView(args.TargetURL).ShowDialog()));
                    args.Cancel = true;
                }
            } else {
                args.Cancel = true;
                OpenInSystemBrowser(args.TargetURL);
            }
        }
*/

        /*
        static BrowserView CreateBrowserView(IntPtr newViewInstance) {
            var browserView = new BrowserView();
            browserView.Setup(newViewInstance);
            return browserView;
        }

        static BrowserView CreateBrowserView(Uri uri) {
            var browserView = new BrowserView();
            browserView.Setup(uri);
            return browserView;
        }
*/

        static void OpenInSystemBrowser(Uri targetURL) {
            Tools.Generic.TryOpenUrl(targetURL);
        }

        static void SetHeightFromDocument(ChromiumWebBrowser webControl) {
            webControl.EvaluateScriptAsync(@"(function() {
var body = document.body,
    html = document.documentElement;

var height = Math.max( body.scrollHeight, body.offsetHeight, 
                       html.clientHeight, html.scrollHeight, html.offsetHeight );
return height;
})();")
                .ContinueWith(
                    height => {
                        webControl.Height = (int) height.Result.Result + 20; // Take possible scrollbar into acct
                    });
        }

        #region Events

        public static void OnEnableSixChanged(DependencyObject s, DependencyPropertyChangedEventArgs e) {
            var webControl = s as ChromiumWebBrowser;
            if (webControl == null)
                return;

            if ((bool) e.NewValue) {
                webControl.IsBrowserInitializedChanged += WebControlOnInitialized;
                //webControl.Disposed += WebControlOnDisposed;
            } else {
                webControl.IsBrowserInitializedChanged -= WebControlOnInitialized;
                //WebControlOnDeinitialized(webControl, null);
            }
        }

        /*        static void WebControlOnDisposed(object sender, EventArgs eventArgs) {
            var webControl = (ChromiumWebBrowser) sender;
            //webControl.NativeViewInitialized -= WebControlOnInitialized;
            WebControlOnDeinitialized(webControl, null);
            //webControl.Disposed -= WebControlOnDisposed;
        }

        static void WebControlOnDeinitialized(object sender, RoutedEventArgs e) {
            var webControl = (ChromiumWebBrowser) sender;
            webControl.ShowCreatedWebView -= OnWebControlOnShowCreatedWebView;
            webControl.LoadingFrameComplete -= WebControlOnLoadingFrameComplete;
            webControl.LoadingFrameFailed -= WebControlOnLoadingFrameFailed;
            webControl.LoginRequest -= WebControl_LoginRequest;
        }*/

        #endregion

        /*

        static void NavigationInterceptorOnBeginNavigation(object sender, NavigationEventArgs args) {
            if (CommonUrls.IsWithSixUrl(args.Url) || IsAuthUrl(args.Url))
                return;
            OpenInSystemBrowser(args.Url);
            args.Cancel = true;
        }
*/

        #region Properties

        public static readonly DependencyProperty EnableSixProperty =
            DependencyProperty.RegisterAttached("EnableSix", typeof (bool), typeof (SixWebControlBehavior),
                new UIPropertyMetadata(default(bool), OnEnableSixChanged));
        public static readonly DependencyProperty EnableAutoHeightProperty =
            DependencyProperty.RegisterAttached("EnableAutoHeight", typeof (bool), typeof (SixWebControlBehavior),
                new UIPropertyMetadata(default(bool)));

        public static bool GetEnableSix(DependencyObject obj) {
            return (bool) obj.GetValue(EnableSixProperty);
        }

        public static void SetEnableSix(DependencyObject obj, bool value) {
            obj.SetValue(EnableSixProperty, value);
        }

        public static bool GetEnableAutoHeight(DependencyObject obj) {
            return (bool) obj.GetValue(EnableAutoHeightProperty);
        }

        public static void SetEnableAutoHeight(DependencyObject obj, bool value) {
            obj.SetValue(EnableAutoHeightProperty, value);
        }

        #endregion
    }
}