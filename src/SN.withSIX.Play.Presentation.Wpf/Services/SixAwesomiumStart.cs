// <copyright company="SIX Networks GmbH" file="SixAwesomiumStart.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using CefSharp;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Presentation.Wpf.Services
{
    public static class SixAwesomiumStart
    {
        static UserSettings _settings;
        static readonly string CustomCSS = @"
body {
  background-color: #f0f0f0;
}
::-webkit-scrollbar {
  width: 13px;
  height: 16px;
}

::-webkit-scrollbar-track-piece {
  background-color: #f0f0f0;
  background-clip: padding-box;
  border-color: transparant;
}

::-webkit-scrollbar-thumb:vertical {
  min-height: 16px;
  background-color: rgba(0, 0, 0, .2);
  background-clip: padding-box;
  border: 1px solid #eee;
}";

        public static void Initialize(UserSettings settings) {
            _settings = settings;
            Common.Paths.LogPath.MakeSurePathExists();
            var dataPath = Common.Paths.AwesomiumPath;
            dataPath.MakeSurePathExists();


            Cef.OnContextInitialized += WebCoreOnStarted;
            Cef.Initialize(new CefSettings {
                LogFile = Common.Paths.LogPath.GetChildFileWithName("awesomium.log").ToString(),
                CachePath = dataPath.ToString() //,
                //PersistSessionCookies = true // flash but security hole..
#if DEBUG
                ,
                //RemoteDebuggingHost = "127.0.0.1",
                RemoteDebuggingPort = 6666
#endif
            });
        }

        static void WebCoreOnStarted() {
            //WebCore.ResourceInterceptor = new SixResourceInterceptor(_settings);
        }

        [STAThread]
        public static void Exit() {
            if (Cef.IsInitialized)
                Cef.Shutdown();
        }
    }
}